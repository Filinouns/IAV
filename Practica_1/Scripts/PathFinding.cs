namespace Game_Tank {

    using System.Collections.Generic;
    using System.Diagnostics;
    using UnityEngine;
    using UnityEngine.UI;

    /*
     Hay 3 tipos de busqueda:
        - Busca el objetivo sin tener en cuenta los valores de las casillas, usa una mala euristica a la 
        hora de calcular el fCost.
        - Utiliza listas para encontrar el objetivo, implementa correctamente la busqueda mas optima.
        - Utiliza la clase "Heap" para abaratar costes de ejecucion utilizando un arbol de Casillas en 
        lugar de una lista por lo que las busquedas son mas rapidas y eficientes.
     */

    public class PathFinding : MonoBehaviour {

        public Casilla seeker, target;

        public Tablero tablero_;

        private int mode_;

        public Text TiempoText_;

        private bool working = false;

        public void setWorking(bool b) { working = b; }

        [HideInInspector] public float cost;

        bool mode2 = false;

        private void Start() {
            TiempoText_.text = "Tiempo: " + 0 + "ms";
            cost = 0;
        }

        void Update() {
            search();
        }

        // Realiza la busqueda
        private void search()
        {
            if (Input.GetButtonDown("Jump"))
            {
                this.setWorking(true);
                mode_ = tablero_.getTank().getMode();
                seeker = tablero_.getTank();
                target = tablero_.getCandy();
                if (seeker != null && target != null && working)
                {
                    selectMode();
                    tablero_.getTank().setSteps(0);
                }
            }
        }

        // Selecciona el modo de busqueda
        private void selectMode() {
            UnityEngine.Debug.Log(ToString() + "Activando modo: " + mode_);
            switch (mode_) {
                case 0: //Busqueda con ManhattanDistance y con heap
                    FindPath(seeker.pos, target.pos);
                    break;
                case 1: //Busqueda con listas 
                    SlowerFindingPath(seeker.pos, target.pos);
                    break;
                case 2: //Busqueda con Heap 
                    mode2 = true; //con este mode activo una heurística u otra
                    FindPath(seeker.pos, target.pos);
                    break;
                default:
                    UnityEngine.Debug.Log(ToString() + "Acho que no me has dicho que ase");
                    break;
            }
        }

        // Busqueda por el algoritmo de A* busca en las 4-8 posiciones adjuntas a la posicion de busqueda
        // "los 4-8 vecinos" y calcula por cual de ellos es mas rapido llegar a la meta, si es que se puede
        // pasar por ellos claro.
        // Dependiendo del modo activado tiene en cuenta el coste de avanzar por una casilla o no
        private void FindPath(Position initPos, Position targetPos) {

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Casilla startCasilla = tablero_.GetBlock(initPos);
            Casilla targetCasilla = tablero_.GetBlock(targetPos);

            //List<Casilla> openSet = new List<Casilla>();
            
            Heap<Casilla> openSet = new Heap<Casilla>(tablero_.MaxSize);
            HashSet<Casilla> closedSet = new HashSet<Casilla>();
            openSet.Add(startCasilla);

            while(openSet.Count > 0) {
                Casilla currentCasilla = openSet.RemoveFirst();

                closedSet.Add(currentCasilla);

                //En caso de que lleguemos al caramelo
                if (currentCasilla == targetCasilla) {
                    sw.Stop();
                    UnityEngine.Debug.Log(ToString() + "Path found: " + sw.ElapsedMilliseconds + " ms");

                    TiempoText_.text = "Tiempo: " + sw.ElapsedMilliseconds + "ms";
                    RetracePath(startCasilla, targetCasilla);
                    return;
                }

                //Mirar los vecinos de cada Casilla que exploramos
                //foreach(Casilla neighbour in tablero_.Get8Neighbours(currentCasilla)) {   // Para 8 vecinos
                foreach (Casilla neighbour in tablero_.Get4Neighbours(currentCasilla)) { // Para 4 vecinos 
                    //Si es muro o default pasamos la iteracion
                    if (neighbour.type_ == 3 || neighbour.type_ == 6 || closedSet.Contains(neighbour) || neighbour == null) {
                        continue;
                    }

                    // A partir de aqui se calcula el coste de movimiento (hCost, gCost) 
                    int newMovementCostToNeighbour;
                    //--------------------Modo 0-------------------
                    if (!mode2) {
                        newMovementCostToNeighbour = currentCasilla.gCost + ManhattanDistance(currentCasilla, neighbour) + neighbour.penalty *10;

                        if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                        {
                            neighbour.gCost = newMovementCostToNeighbour;
                            neighbour.hCost = ManhattanDistance(neighbour, targetCasilla);
                            neighbour.parent = currentCasilla;

                            if (!openSet.Contains(neighbour))
                            { // Si ya lo tiene esque ha cambiado el valor de esa casilla
                                openSet.Add(neighbour);
                            }
                            else // Actualizamos la casilla
                            {
                                openSet.UpdateItem(neighbour);
                            }
                        }
                    }
                    //---------------------Modo 2------------------
                    else  {
                        newMovementCostToNeighbour = currentCasilla.gCost + GetDistance(currentCasilla, neighbour) + neighbour.penalty;

                        if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                        {
                            //cost += newMovementCostToNeighbour;
                            neighbour.gCost = newMovementCostToNeighbour;
                            neighbour.hCost = GetDistance(neighbour, targetCasilla);
                            neighbour.parent = currentCasilla;

                            if (!openSet.Contains(neighbour))
                            { // Si ya lo tiene esque ha cambiado el valor de esa casilla
                                openSet.Add(neighbour);
                            }
                            else // Actualizamos la casilla
                            {
                                openSet.UpdateItem(neighbour);
                            }
                        }
                    }
                }
            }
        }

        // Implementa el mismo A* que el metodo de arriba, solo que en vez de usar Heap usa Listas, lo que incrementa el coste de ejecucion
        private void SlowerFindingPath(Position iPos, Position fPos)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            Casilla startCasilla = tablero_.GetBlock(iPos);
            Casilla targetCasilla = tablero_.GetBlock(fPos);

            List<Casilla> openSet = new List<Casilla>();
            HashSet<Casilla> closedSet = new HashSet<Casilla>();
            openSet.Add(startCasilla);

            while (openSet.Count > 0)
            {
                Casilla currentCasilla = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    // Coomparamos los fCost de las casillas
                    if (openSet[i].fCost < currentCasilla.fCost || openSet[i].fCost == currentCasilla.fCost && openSet[i].hCost < currentCasilla.hCost)
                    {
                        currentCasilla = openSet[i];
                    }
                }

                openSet.Remove(currentCasilla);
                closedSet.Add(currentCasilla);

                //En caso de que lleguemos al caramelo
                if (currentCasilla == targetCasilla)
                {
                    sw.Stop();
                    UnityEngine.Debug.Log(ToString() + "Path found: " + sw.ElapsedMilliseconds + " ms");

                    TiempoText_.text = "Tiempo: " + sw.ElapsedMilliseconds + "ms";
                    RetracePath(startCasilla, targetCasilla);
                    return;
                }

                foreach (Casilla neighbour in tablero_.Get4Neighbours(currentCasilla))
                {
                    //Si es muro o default pasamos la iteracion
                    if (neighbour.type_ == 3 || neighbour.type_ == 6 || closedSet.Contains(neighbour) || neighbour == null)
                    {
                        continue;
                    }

                    // A partir de aqui se calcula el coste de movimiento (hCost, gCost y penalty)
                    int newMovementCostToNeighbour = currentCasilla.gCost + GetDistance(currentCasilla, neighbour) + neighbour.penalty;

                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetCasilla);
                        neighbour.parent = currentCasilla;

                        if (!openSet.Contains(neighbour))
                        { // Si ya lo tiene esque ha cambiado el valor de esa casilla
                            openSet.Add(neighbour);
                        }
                    }
                }
            }
        }

        // Te devuelve el camino que deves seguir dando la vuelta a la lista de casillas que se ha ido guardando
        private void RetracePath(Casilla startCasilla, Casilla endCasilla) {
            List<Casilla> path = new List<Casilla>();
            Casilla currentCasilla = endCasilla;

            while (currentCasilla != startCasilla) {
                cost += currentCasilla.penalty;
                path.Add(currentCasilla);
                currentCasilla = currentCasilla.parent;
            }
            path.Reverse();
            tablero_.setPath(path);
        }

        //Calculo del coste de movimiento por ManhattanDistance
        private int ManhattanDistance(Casilla a, Casilla b) {
            return 10 * Mathf.RoundToInt(Mathf.Abs(a.pos.GetRow() - b.pos.GetRow()) + Mathf.Abs(a.pos.GetColumn() - b.pos.GetColumn()));
        }

        //Calculo original del coste de movimiento
        private int GetDistance(Casilla a, Casilla b) {
            int dstX = Mathf.RoundToInt(Mathf.Abs(a.pos.GetRow() - b.pos.GetRow()));
            int dstY = Mathf.RoundToInt(Mathf.Abs(a.pos.GetColumn() - b.pos.GetColumn()));

            if (dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY);
            else
                return 14 * dstX + 10 * (dstY - dstX);
        }

        private int PenaltyDistance(Casilla a, Casilla b) {
            int dstX = Mathf.RoundToInt(Mathf.Abs(a.pos.GetRow() - b.pos.GetRow()));
            int dstY = Mathf.RoundToInt(Mathf.Abs(a.pos.GetColumn() - b.pos.GetColumn()));
            int penalty = b.penalty;

            if (dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY) + 10 * penalty;
            else
                return 14 * dstX + 10 * (dstY - dstX) + 10 * penalty;
        }
    }
}