using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Technical_Assesment;
using Technical_Assesment.Sorting;

namespace UnitTestProject1
{
    [TestClass]
    public class BTreeTest
    {
        [TestMethod]
        public void CanCreateTree()
        {
            ISortable<Person> routine = new PersonNameSort();
            BTree<Person> myTree = new BTree<Person>(routine);
 
            Assert.IsTrue(myTree != null, "Null tree object, faulty logic ;(");
        }

        [TestMethod]
        public void CanCreateTreeWithRoot()
        {
            ISortable<Person> routine = new PersonNameSort();
            Person p = PersonFactory.CreatePerson("Bob", "Smith", 35);

            Node<Person> myNode = new Node<Person>(p);

            BTree<Person> myTree = new BTree<Person>(myNode, routine);

            Assert.IsTrue(myTree != null, "Null tree object, faulty logic ;(");
            Assert.AreEqual(1, myTree.NodeCount, "Failed to find the one node added to the tree");
        }

        [TestMethod]
        public void CanAddNode()
        {
            ISortable<Person> routine = new PersonNameSort();
            Person p = PersonFactory.CreatePerson("Bob", "Smith", 35);

            Node<Person> myNode = new Node<Person>(p);

            BTree<Person> myTree = new BTree<Person>(myNode, routine);

            p = PersonFactory.CreatePerson("Jess", "Doe", 27);

            myTree.Add(p);

            Assert.IsTrue(myTree != null, "Null tree object, faulty logic ;(");
            Assert.AreEqual(2, myTree.NodeCount, "Failed to find the two nodes added to the tree");
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DuplicateNodeThrowsExceptionWithNameAlgo()
        {
            ISortable<Person> routine = new PersonNameSort();
            Person p = PersonFactory.CreatePerson("Bob", "Smith", 35);

            Node<Person> myNode = new Node<Person>(p);

            BTree<Person> myTree = new BTree<Person>(myNode, routine);

            p = PersonFactory.CreatePerson("Bob", "Smith", 27);

            myTree.Add(p);

            Assert.Fail("No exception on duplicate insert ;(");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DuplicateNodeThrowsExceptionWithNameAgeAlgo()
        {
            ISortable<Person> routine = new PersonAgeSort();
            Person p = PersonFactory.CreatePerson("Bob", "Smith", 35);

            Node<Person> myNode = new Node<Person>(p);

            BTree<Person> myTree = new BTree<Person>(myNode, routine);

            p = PersonFactory.CreatePerson("Bob", "Smith", 35);

            myTree.Add(p);

            Assert.Fail("No exception on duplicate insert ;(");
        }

        [TestMethod]
        public void CanFindPerson()
        {
            ISortable<Person> routine = new PersonNameSort();
            Person p = PersonFactory.CreatePerson("Bob", "Smith", 35);

            Node<Person> myNode = new Node<Person>(p);

            BTree<Person> myTree = new BTree<Person>(myNode, routine);

            p = PersonFactory.CreatePerson("Jess", "Doe", 27);

            myTree.Add(p);

            Assert.IsTrue(myTree.Contains(p), "Person is not found in the tree ;(");
        }

        [TestMethod]
        public void CanFailToFindPerson()
        {
            ISortable<Person> routine = new PersonNameSort();
            Person p = PersonFactory.CreatePerson("Bob", "Smith", 35);

            Node<Person> myNode = new Node<Person>(p);

            BTree<Person> myTree = new BTree<Person>(myNode, routine);

            p = PersonFactory.CreatePerson("Jess", "Doe", 27);

            Assert.IsFalse(myTree.Contains(p), "Person not added is found in the tree ;(");
        }

        [TestMethod]
        public void CanFailToFindPersonWhenNoRoot()
        {
            ISortable<Person> routine = new PersonNameSort();
            Person p = PersonFactory.CreatePerson("Bob", "Smith", 35);

            BTree<Person> myTree = new BTree<Person>(routine);

            Assert.IsFalse(myTree.Contains(p), "Person not added is found in the tree ;(");
        }

        [TestMethod]
        public void CanRemovePersonWhenRoot()
        {
            ISortable<Person> routine = new PersonNameSort();
            Person p = PersonFactory.CreatePerson("Bob", "Smith", 35);

            Node<Person> myNode = new Node<Person>(p);

            BTree<Person> myTree = new BTree<Person>(myNode, routine);

            myTree.Remove(p);

            Assert.IsFalse(myTree.Contains(p), "Person removed is found in the tree ;(");
        }

        [TestMethod]
        public void CanRemovePersonWhenLeaf()
        {
            ISortable<Person> routine = new PersonNameSort();
            Person p0 = PersonFactory.CreatePerson("Bob", "Smith", 35);

            Node<Person> myNode = new Node<Person>(p0);

            BTree<Person> myTree = new BTree<Person>(myNode, routine);

            Person p = PersonFactory.CreatePerson("Jane", "Doe", 27);

            myTree.Add(p);

            myTree.Remove(p);

            Assert.IsFalse(myTree.Contains(p), "Person removed is found in the tree ;(");
        }

        // Silly method used to generate the records.txt file for the assesment ;)
        //[TestMethod]
        //public void Foo()
        //{

        //    string[] fnames = File.ReadAllLines(@"C:\Users\travis.frisinger\Desktop\raw_name_list.txt");
        //    string[] lnames = File.ReadAllLines(@"C:\Users\travis.frisinger\Desktop\raw_surnames_list.txt");

        //    int fnamePos = 0;
        //    int lnamePos = 0;

        //    Random r = new Random(100);

        //    int max = 1000000;
        //    int cnt = 0;

        //    while(cnt < max)
        //    {

        //        int age = r.Next(18, 122);

        //        string fname = fnames[fnamePos];
        //        string lname = lnames[lnamePos];

        //        // now dump to file ;)
        //        if (fname.Length > 1 && lname.Length > 1 && fname.IndexOf(@"\") < 0 && fname.IndexOf("/") < 0 && lname.IndexOf(@"\") < 0 && lname.IndexOf("/") < 0)
        //        {
        //            File.AppendAllText(@"F:\foo\records.txt", fname + "," + char.ToUpper(lname[0]) + lname.Substring(1).Replace("^","").Trim() + "," + age + Environment.NewLine);
        //            cnt++;
        //        }


        //        fnamePos += r.Next(1, 500);
        //        lnamePos += r.Next(1, 350);

        //        if (fnamePos >= fnames.Length)
        //        {
        //            fnamePos = 0;
        //        }

        //        if (lnamePos >= lnames.Length)
        //        {
        //            lnamePos = 0;
        //        }
        //    }
        //}
    }
}
