using Spectre.Console.Rendering;

public class CustomRenderable(IEnumerable<Segment> segments) : IRenderable
{
    private readonly List<Segment> _segments = segments.ToList();

    public Measurement Measure(RenderOptions options, int maxWidth)
    {
        var contentLength = _segments.Sum(s => s.Text.Length);
        return new Measurement(contentLength, contentLength);
    }

    public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        return _segments;
    }
}