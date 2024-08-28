using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SpatialGame
{
    public static class DebugSimulation
    {

        public static void Init()
        {
            
        }

        public static void Update()
        {
            CheckLostElements();
        }

        /// <summary>
        /// in mb
        /// </summary>
        /// <returns></returns>
        public static float GetCurrentMemoryOfSim()
        {
            int size = ElementSimulation.elements.Length;
            size *= Element.GetSize();
            return size / 1024f / 1024f;
        }


        /// <summary>
        /// Check if any pixel elements got deleted from some behavior for when it should have not been deleted
        /// </summary>
        static void CheckLostElements()
        {

            int count = 0;

            for (int i = 0; i < ElementSimulation.elements.Length; i++)
            {
                if (ElementSimulation.elements[i] is null)
                    continue;

                count++;
            }

            int totalElementCount = 0;
            for (int i = 0; i < ElementSimulation.positionCheck.Length; i++)
            {
                int type = ElementSimulation.positionCheck[i];
                if (type == 0)
                    continue;
                totalElementCount++;
            }

            if (ElementSimulation.elements.Length - ElementSimulation.freeElementSpots.Count != totalElementCount)
            {
                Console.WriteLine(totalElementCount + " vis " + count + " nn " + (ElementSimulation.elements.Length - ElementSimulation.freeElementSpots.Count) + " qd");
                //throw new Exception("Weird bullshit has happened there are more elements than on screen");
            }
        }
    }
}
