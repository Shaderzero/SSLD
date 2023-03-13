namespace SSLD.Tools;

public class NameObject
{
    public string Name { get; set; }

    public NameObject(string name)
    {
        this.Name = name;
    }
        
    public NameObject()
    {
        this.Name = "";
    }

    public static List<NameObject> StringToObject(IEnumerable<string> list)
    {
        return list.Select(name => new NameObject(name)).ToList();
    }

    public static List<string> ObjectToString(IEnumerable<NameObject> list)
    {
        return list?.Select(x => x.Name).ToList();
    }
}