using System;
using System.Collections.Generic;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

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
        private List<Food> _favouriteFoodsProperty;
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
        public List<int> BuildInts(int a, int b, int c, int d)
        {
            return new List<int>() { a, b, c, d };
        }

        public List<Food> FavouriteFoods()
        {
            return _favouriteFoodsProperty;
        }

        public List<Food> FavouriteFoodsProperty
        {
            get
            {
                return _favouriteFoodsProperty ?? (_favouriteFoodsProperty = new List<Food> { new Food { FoodName = "Pizza" }, new Food { FoodName = "Burger" }, new Food { FoodName = "Chicken" } });
            }
            private set
            {
                _favouriteFoodsProperty = value;
            }
        }
        public List<int> PhoneNumbers()
        {
            var numbers = new List<int> { 1284561478, 228561478, 215561475 };
            return numbers;
        }


        public string EmptyIsNullTest(string value)
        {
            return value ?? "Null value passed";
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

        public Human CopyHuman(Human human)
        {
            return new Human(human.Name, human.SurName, human.PersonFood);
        }

        public string AddFavouriteFood(Food food)
        {
            if (food == null)
            {
                throw new Exception("Please specify favourite food");
            }
            _favouriteFoodsProperty.Add(food);
            return "Success";
        }

        public string RemoveFavouriteFood(Food food)
        {
            var removeAll = _favouriteFoodsProperty.RemoveAll(food1 => food1.FoodName.Equals(food.FoodName, StringComparison.InvariantCultureIgnoreCase));
            return $"removed {removeAll} foods";
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
