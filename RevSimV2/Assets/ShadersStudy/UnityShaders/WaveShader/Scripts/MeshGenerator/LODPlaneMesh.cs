using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JustPtrck.Mesh {

    public class LODPlaneMesh : MonoBehaviour
    {
        // TEMP Class definition
        [SerializeField]//, ToolTip("Amount of units before halved mesh resolution")]
        private List<int> LODSize = new List<int>();
        [SerializeField, Range(1, 10)]//, ToolTip("Amount of quads per unit at LOD0")]
        private int LOD0Resolution = 1; 


        private void Update() {
            CreateLODMesh();
        }

        /// <summary>
        /// This Method is used to create a LOD mesh        <para/>
        /// LIST CreateLODMesh Updates                            <br/>
        /// [ ]: Generate Triangles using the vertices      <br/>
        /// [ ]: Make sure LOD1 and up works                            <br/>
        /// [ ]: Make this Method callable, no class        <br/>
        /// </summary>
        private void CreateLODMesh()
        {
            int v = 0;
            float x = 0;
            float z = 0;
            int LOD_lvl = 0;
            List<Vector3> vertices = new List<Vector3>();
            vertices.Add(new Vector3(x, 0, z)); 
            v++;
            int lunits = 0;
            foreach (int units in LODSize)
            {
                float mod = 1f/(float)LOD0Resolution * Mathf.Pow(2f, (float)LOD_lvl);
                if (LOD_lvl == 0){
                    for (int u = 0; u <= ( units) * LOD0Resolution; u++)
                    {
                        for(int i = 0; i < u * 8; i++)
                        {
                            if (i == 0)                                  
                                z+= mod;
                            else if (i <= (u * 2 - 1))                            
                                x+= mod;
                            else if (i > (u * 2 - 1)  && i <= (u * 4 - 1) )        
                                z-= mod;
                            else if (i > (u * 4 - 1)  && i <= (u * 6 - 1) )     
                                x-= mod;
                            else if (i > (u * 6 - 1)  && i <= (u * 8 - 1) )    
                                z+= mod;

                            vertices.Add(new Vector3(x, 0, z)) ; 
                            if (v > 0) Debug.DrawLine(vertices[v], vertices[v-1], Color.red);
                            v++;
                        }
                    }
                    lunits = units;
                }
                // IDEA Find a pattern in LOD levels
                // u_LOD0start = 0                      (LOD_lvl + lunits/2) --> (0 + 0 / 2)            Works!
                // u_LOD1start = 1 + LOD0size / 2       (LOD_lvl + lunits/2) --> (1 + LOD0size / 2)     Works!
                // u_LOD2start = ???
                else if (LOD_lvl == 1){
                    for (int u = 1 + lunits/(2) ; u <= ( units + 1 + lunits/(2)) * LOD0Resolution; u++)
                    {
                        for(int i = 0; i < u * 8; i++)
                        {
                            if (i == 0)                                  
                                z+= mod;
                            else if (i <= (u * 2 - 1))                            
                                x+= mod;
                            else if (i > (u * 2 - 1)  && i <= (u * 4 - 1) )        
                                z-= mod;
                            else if (i > (u * 4 - 1)  && i <= (u * 6 - 1) )     
                                x-= mod;
                            else if (i > (u * 6 - 1)  && i <= (u * 8 - 1) )    
                                z+= mod;

                            vertices.Add(new Vector3(x, 0, z)) ; 
                            if (v > 0) Debug.DrawLine(vertices[v], vertices[v-1], Color.red);
                            v++;
                        }
                    }
                    lunits = lunits / 2 + units;
                }
                else if (LOD_lvl == 2){
                    for (int u = 1 + lunits/(2) ; u <= ( units + 1 + lunits/(2)) * LOD0Resolution; u++)
                    {
                        for(int i = 0; i < u * 8; i++)
                        {
                            if (i == 0)                                  
                                z+= mod;
                            else if (i <= (u * 2 - 1))                            
                                x+= mod;
                            else if (i > (u * 2 - 1)  && i <= (u * 4 - 1) )        
                                z-= mod;
                            else if (i > (u * 4 - 1)  && i <= (u * 6 - 1) )     
                                x-= mod;
                            else if (i > (u * 6 - 1)  && i <= (u * 8 - 1) )    
                                z+= mod;

                            vertices.Add(new Vector3(x, 0, z)) ; 
                            if (v > 0) Debug.DrawLine(vertices[v], vertices[v-1], Color.red);
                            v++;
                        }
                    }
                    lunits = lunits / 2 + units;
                }
                LOD_lvl++;
            }


        }
    }
}