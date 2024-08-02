using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
    public static class DebugSimulation
    {

        public static Dictionary<int, int> currentElementCount;

        public static void Init()
        {
            currentElementCount = new Dictionary<int, int>();
        }

        public static void Update()
        {
            CheckLostElements();
        }


        /// <summary>
        /// Check if any pixel elements got deleted from some behavior for when it should have not been deleted
        /// </summary>
        static void CheckLostElements()
        {
            Dictionary<int, int> beforeElementCount = currentElementCount;
            UpdateElementCount();

            foreach (var value in beforeElementCount)
            {
                //put in imgui
                Console.WriteLine("Element " + value.Key + " has " + value.Value);
                if (currentElementCount[value.Key] < value.Value)
                {
                    Console.WriteLine("Lost element of type " + value.Key);
                }
            }
        }



        static void UpdateElementCount()
        {
            currentElementCount.Clear();

            int currentId = 0;
            for (int i = 0; i < ElementSimulation.elements.Count; i++)
            {
                if(currentId != ElementSimulation.elements[i].GetElementType().ToUshort())
                {
                    currentId = ElementSimulation.elements[i].GetElementType().ToUshort();
                    if(!currentElementCount.TryAdd(currentId, 0))
                    {
                        //we hit a error where it tried to add a key that is there
                        //but should not because its cleared but it still crashes sometimes
                        //so this is the fix
                        currentElementCount.Clear();
                        return;
                    }
                }
            }

            for (int i = 0; i < ElementSimulation.positionCheck.Length; i++)
            {
                int type = ElementSimulation.positionCheck[i];
                if (type == 0)
                    continue;
                if(currentElementCount.TryGetValue(type, out int value))
                {
                    //moving the add like value++ does not add one to it for some reason
                    //possible c# bug?
                    currentElementCount[type] = ++value;
                }
            }
        }
    }
}
