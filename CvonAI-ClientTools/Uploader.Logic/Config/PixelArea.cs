namespace Uploader.Logic.Config
{
    /// <summary>
    /// Describes a pixel are in the <see cref="ImageModalityConfiguration"/> of a project's <see cref="Site"/> configuration
    /// </summary>1
    public class PixelArea
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        // TODO: add scaling?
    }
}