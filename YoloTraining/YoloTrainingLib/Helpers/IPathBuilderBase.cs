namespace YoloTrainingLib.Helpers
{
    public interface IPathBuilderBase
    {
        string BaseDataPath { get; set; }
        string Combine(params string[] parts);
        string GetCategoryPath(string category);

    }
}