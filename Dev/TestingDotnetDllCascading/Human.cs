using System;

namespace TestingDotnetDllCascading
{
    [Serializable]
    public class Human
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string Name { get; set; }
        public Human()
        {
            Name = "Default";
            PersonFood = new Food()
            {
                FoodName = "DefaultFood"
            };
        }

        public Human(string name)
        {
            Name = name;
            PersonFood = new Food()
            {
                FoodName = "DefaultFood"
            };
        }

        public Human(string name, string surname, Food food)
        {
            
            Name = name;
            SurName = surname;
            PersonFood = food;
        }

        #region Overrides of Object

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return $"Name:{Name}, Surname:{SurName}, FoodName:{PersonFood.FoodName}";
        }

        #endregion

        public Food PersonFood { get; set; }
        public string SurName { get; set; }
    }

    public class Food
    {
        public string FoodName { get; set; }
    }
}
