namespace RevitAddinCSharp.UI
{
    public class LevelTypeOption
    {
        public string Name { get; }
        public long Id { get; }

        public LevelTypeOption(string name, long id)
        {
            Name = name;
            Id = id;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
