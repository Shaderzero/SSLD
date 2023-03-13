namespace SSLD.Tools;

public static class NameObjectExtensions
{
    public static List<string> ToListString(this IEnumerable<NameObject> list)
    {
        return list?.Select(x => x.Name).ToList();
    }
    
    public static List<NameObject> ToListObject(this IEnumerable<string> list)
    {
        return list.Select(name => new NameObject(name)).ToList();
    }
}