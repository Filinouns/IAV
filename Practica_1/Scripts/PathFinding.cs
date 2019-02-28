namespace Game_Tank {

    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using UnityEngine;

    public class PathFinding : MonoBehaviour {

        public Casilla seeker, target;

        //public void setSeek(Tra p) { seeker = p; }
        //public void setTarg(Position p) { target = p; }

        public Tablero tablero_;

        void Update() {

            if (Input.GetButtonDown("Jump")) {
                seeker = tablero_.getTank();
                target = tablero_.getCandy();
                if (seeker != null && target != null) {
                    FindPath(seeker.pos, target.pos);
                }
            }
        }

        // Bicho tocho A*
        private void FindPath(Position initPos, Position targetPos) {

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Casilla startCasilla = tablero_.GetBlock(initPos);
            Casilla targetCasilla = tablero_.GetBlock(targetPos);

            UnityEngine.Debug.Log(ToString() + "Incredibile1");

            //List<Casilla> openSet = new List<Casilla>();
            //El error ocurre al inicializar el Heap
            Heap<Casilla> openSet = new Heap<Casilla>(tablero_.MaxSize);
            HashSet<Casilla> closedSet = new HashSet<Casilla>();
            openSet.Add(startCasilla);

            UnityEngine.Debug.Log(ToString() + "Incredibile2");

            while(openSet.Count > 0) {
                Casilla currentCasilla = openSet.RemoveFirst();

                //Esta es la parte mas costosa del algoritmo (solo con listas)
                /*
                Casilla currentCasilla = openSet[0];
                for (int i = 1; i < openSet.Count; i++) {
                    // Coomparamos los fCost de las casillas
                    if (openSet[i].fCost < currentCasilla.fCost || openSet[i].fCost == currentCasilla.fCost && openSet[i].hCost < currentCasilla.hCost) {
                        currentCasilla = openSet[i];
                    }
                }

                //Hasta aqui

                openSet.Remove(currentCasilla);
                */
                closedSet.Add(currentCasilla);

                //En caso de que lleguemos al caramelo
                if (currentCasilla == targetCasilla) {
                    sw.Stop();
                    UnityEngine.Debug.Log(ToString() + "Path found: " + sw.ElapsedMilliseconds + " ms");

                    RetracePath(startCasilla, targetCasilla);
                    return;
                }

                //Mirar los vecinos de cada Casilla que exploramos
                foreach(Casilla neighbour in tablero_.Get8Neighbours(currentCasilla)) {   // Para 8 vecinos
                // Para 4 vecinos
                /*foreach (Casilla neighbour in tablero_.Get4Neighbours(currentCasilla)) {    
                    int debugg = 0;
                    Debug.Log(ToString() + "Posible vecino " + debugg + ": " + neighbour.pos);
                    debugg++;
                */

                    //Si es muro o default pasamos la iteracion
                    if (neighbour.type_ == 3 || neighbour.type_ == 6 || closedSet.Contains(neighbour) || neighbour == null) {
                        continue;
                    }

                    int newMovementCostToNeighbour = currentCasilla.gCost + GetDistance(currentCasilla, neighbour);

                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)) {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetCasilla);
                        neighbour.parent = currentCasilla;

                        if (!openSet.Contains(neighbour)) {
                            openSet.Add(neighbour);
                        }
                    }
                }
            }
        }

        private void RetracePath(Casilla startCasilla, Casilla endCasilla) {
            List<Casilla> path = new List<Casilla>();
            Casilla currentCasilla = endCasilla;

            while (currentCasilla != startCasilla) {
                path.Add(currentCasilla);
                currentCasilla = currentCasilla.parent;
            }
            path.Reverse();
            tablero_.setPath(path);
        }

        private int GetDistance(Casilla a, Casilla b) {
            int dstX = Mathf.RoundToInt(Mathf.Abs(a.pos.GetRow() - b.pos.GetRow()));
            int dstY = Mathf.RoundToInt(Mathf.Abs(a.pos.GetColumn() - b.pos.GetColumn()));

            if (dstX > dstY)
                return 10 * dstY + 10 * (dstX - dstY);
            else
                return 10 * dstX + 10 * (dstY - dstX);
        }

    }
}