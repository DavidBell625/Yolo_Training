import os
import tempfile
import shutil
import base64
import asyncio
import time
import json
from fastapi import FastAPI, File, UploadFile, Query, HTTPException
from fastapi.responses import JSONResponse
from ultralytics import YOLO
from PIL import Image
import pillow_heif
import numpy as np
import cv2
from asyncio import Lock

app = FastAPI(title="YOLOv8 Flexible Model Inference with Preload and Stats")

# 🔹 Global cache and lock to manage models
model_cache = {}
model_lock = Lock()


@app.post("/predict_batch")
async def predict_batch(
    model_folder: str = Query(..., description="Full path to model folder containing best.pt"),
    files: list[UploadFile] = File(...),
    input_folder: str = Query("/data/input", description="Folder to save uploaded images"),
    output_folder: str = Query("/data/output", description="Folder to save annotated images and summary")
):
    if not os.path.isdir(model_folder):
        raise HTTPException(status_code=404, detail=f"Model folder '{model_folder}' not found.")

    model_file = os.path.join(model_folder, "best.pt")
    if not os.path.isfile(model_file):
        raise HTTPException(status_code=404, detail=f"'best.pt' not found in model folder '{model_folder}'.")

    # Ensure input/output folders exist
    os.makedirs(input_folder, exist_ok=True)
    os.makedirs(output_folder, exist_ok=True)

    # 🔹 Load model with thread-safe locking
    async with model_lock:
        if model_folder not in model_cache:
            model_cache[model_folder] = YOLO(model_file)
        model = model_cache[model_folder]

    total_detections = 0
    detections_by_class = {}
    images_with_no_detections = 0
    per_image_stats = []
    results_list = []

    for file in files:
        start_time = time.time()

        _, ext = os.path.splitext(file.filename)
        ext = ext.lower()

        # Save uploaded image permanently
        save_path = os.path.join(input_folder, file.filename)
        with open(save_path, "wb") as f:
            shutil.copyfileobj(file.file, f)

        try:
            pil_img = Image.open(save_path).convert("RGB")
        except Exception:
            raise HTTPException(status_code=400, detail=f"Unable to read image {file.filename}")

        width, height = pil_img.size
        total_pixels = width * height if width and height else 1
        aspect_ratio = round(width / height, 4) if height > 0 else None
        file_size = os.path.getsize(save_path)

        # YOLO inference
        results = model.predict(save_path, verbose=False, iou=0.2, conf=0.4)

        detections = []
        confs = []
        bbox_areas = []
        names = model.names
        img_detections_by_class = {}

        for box in results[0].boxes:
            cls_name = names[int(box.cls)]
            conf_val = float(box.conf)
            x1, y1, x2, y2 = box.xyxy[0]
            bbox_area = max(0, (float(x2) - float(x1))) * max(0, (float(y2) - float(y1)))
            bbox_area_pct = bbox_area / total_pixels if total_pixels else 0

            detections.append({
                "class": cls_name,
                "confidence": round(conf_val, 4),
                "bbox": [round(float(x1), 2), round(float(y1), 2), round(float(x2), 2), round(float(y2), 2)],
                "bbox_area": round(bbox_area, 2),
                "bbox_area_pct": round(bbox_area_pct, 6)
            })

            confs.append(conf_val)
            bbox_areas.append(bbox_area)
            total_detections += 1
            detections_by_class[cls_name] = detections_by_class.get(cls_name, 0) + 1
            img_detections_by_class[cls_name] = img_detections_by_class.get(cls_name, 0) + 1

        if not detections:
            images_with_no_detections += 1

        # Save annotated image to output folder
        annotated_img = results[0].plot()
        output_img_path = os.path.join(output_folder, file.filename)
        cv2.imwrite(output_img_path, annotated_img)

        # Base64 for response
        _, buffer = cv2.imencode(".jpg", annotated_img)
        annotated_b64 = base64.b64encode(buffer).decode("utf-8")

        img_stats = {
            "filename": file.filename,
            "size_bytes": file_size,
            "image_width": width,
            "image_height": height,
            "aspect_ratio": aspect_ratio,
            "total_pixels": total_pixels,
            "detections_count": len(detections),
            "detections_by_class": img_detections_by_class,
            "confidence_min": round(min(confs), 4) if confs else None,
            "confidence_max": round(max(confs), 4) if confs else None,
            "confidence_avg": round(sum(confs) / len(confs), 4) if confs else None,
            "bbox_area_min": round(min(bbox_areas), 2) if bbox_areas else None,
            "bbox_area_max": round(max(bbox_areas), 2) if bbox_areas else None,
            "bbox_area_avg": round(sum(bbox_areas) / len(bbox_areas), 2) if bbox_areas else None,
            "bbox_area_avg_pct": round((sum(bbox_areas) / len(bbox_areas)) / total_pixels, 6) if bbox_areas else None,
            "detection_density_per_mp": round(len(detections) / (total_pixels / 1_000_000), 6) if total_pixels else None,
            "processing_time_sec": round(time.time() - start_time, 4)
        }
        per_image_stats.append(img_stats)

        results_list.append({
            "image": file.filename,
            "detections": detections,
            "annotated_image": annotated_b64,
            "stats": img_stats,
            "saved_input_path": save_path,
            "saved_output_path": output_img_path
        })

    # Save summary JSON
    summary_path = os.path.join(output_folder, "prediction_summary.json")
    with open(summary_path, "w") as f:
        json.dump({
            "stats": {
                "total_images": len(files),
                "total_detections": total_detections,
                "detections_by_class": detections_by_class,
                "images_with_no_detections": images_with_no_detections,
                "per_image_stats": per_image_stats
            },
            "results": results_list
        }, f, indent=4)

    return JSONResponse(content={
        "stats": {
            "total_images": len(files),
            "total_detections": total_detections,
            "detections_by_class": detections_by_class,
            "images_with_no_detections": images_with_no_detections,
            "per_image_stats": per_image_stats
        },
        "results": results_list,
        "summary_json_path": summary_path
    })
