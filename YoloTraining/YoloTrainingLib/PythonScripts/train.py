import argparse
from ultralytics import YOLO
import os
import json
import sys

sys.stdout.reconfigure(line_buffering=True)

def parse_args():
    parser = argparse.ArgumentParser(description="Train or fine-tune a YOLOv8 model with Ultralytics")
    parser.add_argument('--dataset_dir', type=str, required=True,
                        help='Path to dataset root (must contain data.yaml and images/labels)')
    parser.add_argument('--experiment_name', type=str, required=True, help='Experiment name')
    parser.add_argument('--max_epochs', type=int, default=100)
    parser.add_argument('--batch_size', type=int, default=16)
    parser.add_argument('--learning_rate', type=float, default=0.01)
    parser.add_argument('--resume', action='store_true', help='Resume training from latest checkpoint')
    parser.add_argument('--model_size', type=str, choices=["n", "s", "m", "l", "x"], default="s",
                        help='YOLOv8 model size')
    parser.add_argument('--weights', type=str, default=None,
                        help='Path to .pt weights for fine-tuning (overrides model_size)')
    return parser.parse_args()

def on_epoch_end(trainer):
    try:
        epoch = getattr(trainer, 'epoch', None)
        loss_val = None
        if hasattr(trainer, 'loss') and trainer.loss is not None:
            try:
                loss_val = float(trainer.loss)
            except Exception:
                loss_val = None

        log = {
            "type": "progress",
            "epoch": epoch if epoch is not None else -1,
            "loss": loss_val if loss_val is not None else 0.0,
        }
        print(json.dumps(log))
        sys.stdout.flush()
    except Exception as e:
        print(f"[WARN] Failed to emit progress: {e}", file=sys.stderr)
        sys.stderr.flush()

def main():
    args = parse_args()

    # Debug prints
    debug_mode = os.environ.get("DEBUG")
    if debug_mode:
        print(f"[DEBUG] args.weights = '{args.weights}'")
        print(f"[DEBUG] os.path.exists(args.weights) = {os.path.exists(args.weights) if args.weights else 'N/A'}")
        if args.weights:
            print(f"[DEBUG] Absolute path = {os.path.abspath(args.weights)}")

    # Determine model path
    if args.weights and os.path.exists(args.weights):
        model_path = args.weights
        if debug_mode:
            print(f"[DEBUG] Loading existing checkpoint {model_path}", flush=True)
    else:
        # Start from default YOLOv8 model
        model_path = f"yolov8{args.model_size}.pt"
        if debug_mode:
            print(f"[DEBUG] No checkpoint found. Using default model {model_path}", flush=True)

    dataset_yaml = os.path.join(args.dataset_dir, "data.yaml")
    project_dir = os.path.join("runs", args.experiment_name)

    print(json.dumps({"type": "start", "message": f"Training {model_path} on {dataset_yaml}"}))
    sys.stdout.flush()

    # Load the model: always use the provided checkpoint if exists
    model = YOLO(model_path, task="detect")
    model.add_callback("on_train_epoch_end", on_epoch_end)

    results = model.train(
        data=dataset_yaml,
        epochs=args.max_epochs,
        imgsz=640,
        batch=args.batch_size,
        lr0=args.learning_rate,
        resume=args.resume,
        project=project_dir,
        name="exp",
        exist_ok=True,
        workers=0
    )

    metrics = getattr(results, "results_dict", {})
    summary = {
        "type": "result",
        "best_map50": metrics.get("map50", None),
        "best_map": metrics.get("map", None),
        "precision": metrics.get("precision", None),
        "recall": metrics.get("recall", None),
    }
    print(json.dumps(summary))
    sys.stdout.flush()

if __name__ == "__main__":
    main()
