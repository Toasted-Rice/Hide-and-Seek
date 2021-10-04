namespace FIMSpace.Generating
{
    public interface IGenerating
    {
        void Generate();
        /// <summary> Optional preview, can be empty </summary>
        void PreviewGenerate();
    }

}