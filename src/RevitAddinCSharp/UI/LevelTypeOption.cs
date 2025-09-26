namespace RevitAddinCSharp.UI
{
    public class LevelTypeOption
    {
        public string Name { get; }
        public int Id { get; }

        public LevelTypeOption(string name, int id)
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
