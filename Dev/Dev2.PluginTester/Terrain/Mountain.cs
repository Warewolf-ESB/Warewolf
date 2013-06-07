namespace Dev2.Terrain
{
    public class Mountain
    {
        public string Name { get; set; }
        public int Height { get; set; }

        public string Echo(string text)
        {
            return text;
        }

        public Mountain Create(string name, int height)
        {
            return new Mountain
            {
                Name = name,
                Height = height
            };
        }
    }
}
