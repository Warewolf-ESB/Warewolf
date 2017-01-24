using System;
using System.Collections.Generic;

namespace TestingDotnetDllCascading
{
    public sealed class SealedClass
    {
        public override string ToString()
        {
            return "ToStringOnsealed";
        }
    }
    public static class StaticClass
    {
        public static string ToStringOnStatic()
        {
            return "ToStringOnStatic";
        }
    }

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

        public void SetNameInternal()
        {
            Name = "System.Default";
        }

        public List<Food> FavouriteFoods()
        {
            var foods = new List<Food> { new Food { FoodName = "Pizza" }, new Food { FoodName = "Burger" }, new Food { FoodName = "Chicken" } };
            return foods;
        }

        public List<int> PhoneNumbers()
        {
            var numbers = new List<int> { 1284561478, 228561478, 215561475 };
            return numbers;
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


        //Add list of strings
        //add a list of objects
        #endregion

        public Food PersonFood { get; set; }
        public string SurName { get; set; }
    }

    public class Food
    {
        public string FoodName { get; set; }
    }
}
