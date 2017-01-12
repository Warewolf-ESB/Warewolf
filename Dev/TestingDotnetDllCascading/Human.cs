namespace TestingDotnetDllCascading
{
    public class Human
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        string Name { get; set; }
        public Human()
        {
            Name = "Default";
            PersonFood = new Food()
            {
                FoodName = "Cake"
            };
        }

        public Human(string name)
        {
            Name = name;
            PersonFood = new Food()
            {
                FoodName = "Cake"
            };
        }

        public Human(string name, string surname, Food food)
        {
            Name = name;
            SurName = surname;
            PersonFood = food;
        }

        public Food PersonFood { get; set; }
        public string SurName    { get; set; }
    }

    public class Food
    {
       public string FoodName { get; set; }
    }
}
