namespace Game_Tank {

    using System;
    using System.Collections;
    using System.Collections.Generic;

    using UnityEngine;

    public class Tablero : MonoBehaviour {

        private List<Casilla> path_;
        private int rows_, cols_;

        public void setPath(List<Casilla> p) { path_ = p; }

        public int MaxSize {
            get {
                //return casillas_.Length;
                return rows_ * cols_;
            }
        }

        //Constantes
        public static readonly float USER_DELAY = 0.0f;
        public static readonly float AI_DELAY = 0.2f;

        private static readonly float POSITION_FACTOR_R = 1.1f;
        private static readonly float POSITION_FACTOR_C = 1.1f;
        private static readonly float SCALE_FACTOR_R = 1.1f;
        private static readonly float SCALE_FACTOR_C = 1.1f;

        //Prefabs de casillas
        public Casilla casillaPrefab;
        public Casilla sueloPrefab;
        public Casilla muroPrefab;
        public Casilla aguaPrefab;
        public Casilla barroPrefab;
        public Casilla candyPrefab;
        public Tank tankPrefab;

        //GameManager
        private GameManager gm_;

        //PathFinder
        public PathFinding path;

        //Matriz de Casillas
        private Casilla[,] casillas_;

        //Tanke
        private Tank tank_;

        public Tank getTank() { return tank_; }
        public void setTank(Tank t) { tank_ = t; }

        //Casilla de Candy, se guarda por comodidad
        private Casilla candy_;

        public Casilla getCandy() { return candy_; }
        public void setCandy (Casilla c) { candy_ = c; }

        //----------------------------------UPDATE----------------------------

        void Update() {
            if (path_ != null) {
                if (StopMoving()) {
                    //Debug.Log(ToString() + "Parando!");
                    tank_.setObjetive(false);
                    path_ = null;
                }
                else {
                    foreach (Casilla cell in casillas_) {
                        if (path_.Contains(cell))
                        {
                            tank_.setObjetive(true);
                            //showPath(); //Only for debbug
                        }
                    }
                }
            }
        }

        // ----------------------------Public------------------------

        public void Init(GameManager gm, Map m) {
            if (gm == null) throw new ArgumentNullException(nameof(gm));
            if (m == null) throw new ArgumentNullException(nameof(m));

            gm_ = gm;

            // Si el vector de casillas esta sin inicializar lo inicializamos
            if (casillas_ == null) {
                casillas_ = new Casilla[m.rows, m.cols];
                transform.localScale = new Vector3(SCALE_FACTOR_C * casillas_.GetLength(1), transform.localScale.y, SCALE_FACTOR_R * casillas_.GetLength(0));
            }
            // Sino destruimos las existente y creamos uno nuevo
            else if (casillas_.GetLength(0) != m.rows || casillas_.GetLength(1) != m.cols) {
                DestroyCasillas();
                casillas_ = new Casilla[m.rows, m.cols];
                transform.localScale = new Vector3(SCALE_FACTOR_C * casillas_.GetLength(1), transform.localScale.y, SCALE_FACTOR_R * casillas_.GetLength(0));
            }

            rows_ = checked((int)m.rows);
            cols_ = checked((int)m.cols);

            GenerateCasillas(m);

            InitTank();
        }

        //Seleccionar una casilla con Candy
        public void GiveCandy(uint r, uint c) {
            Casilla cell = casillas_[r, c];
            if (cell != null) {
                cell = null;
                Destroy(casillas_[r, c].gameObject);
            }
            cell = Instantiate(candyPrefab,
                           new Vector3(-((casillas_.GetLength(1) / 2.0f) * POSITION_FACTOR_C - (POSITION_FACTOR_C / 2.0f)) + c * POSITION_FACTOR_C,
                                        0,
                                        (casillas_.GetLength(0) / 2.0f) * POSITION_FACTOR_R - (POSITION_FACTOR_R / 2.0f) - r * POSITION_FACTOR_R),
                           Quaternion.identity);
            cell.candy_ = true;

            Position pos = new Position(r, c);
            cell.pos = pos;

            cell.Init(this, 4);
            setCandy(cell);
            casillas_[r, c] = cell;
        }

        // Obtener la casilla mediante una posicion
        public Casilla GetBlock(Position position) {
            if (position == null) throw new ArgumentNullException(nameof(position));

            return casillas_[position.GetRow(), position.GetColumn()];
        }

        //----------------------------Cosas del Tanque--------------------------

        //Comprobacion para terminar el movimiento
        private bool StopMoving() {
            bool move = false;

            if (tank_.getSteps() + 1 >= path_.Capacity) move = true;

            return move;
        }

        //Nos devuelve la posicion del siguiente paso del camino del tank
        public Vector3 MoveTank(int steps) {
            if (path_ == null) throw new InvalidOperationException("This object has not been initialized");

            return path_[steps].transform.position;
        }

        // Metodo para obtener los 8 vecinos de una casilla
        public List<Casilla> Get8Neighbours(Casilla cell)
        {
            List<Casilla> neighbours = new List<Casilla>();

            for (int r = -1; r <= 1; r++) {
                for (int c = -1; c <= 1; c++) {
                    if (r == 0 && c == 0) continue;

                    int checkCol = Mathf.RoundToInt(cell.pos.GetColumn() + r);
                    int checkRow = Mathf.RoundToInt(cell.pos.GetRow() + c);

                    if (checkCol >= 0 && checkCol < casillas_.GetLength(0) && checkRow >= 0 && checkRow < casillas_.GetLength(1))
                    {
                        neighbours.Add(casillas_[checkRow, checkCol]);
                    }
                }
            }
            return neighbours;
        }

        public List<Casilla> Get4Neighbours(Casilla cell)
        {
            List<Casilla> neighbours = new List<Casilla>();

            for (int r = -1; r <= 1; r++)
            {
                if (r == 0) continue;

                int checkCol = Mathf.RoundToInt(cell.pos.GetColumn() + r);

                if (checkCol >= 0 && checkCol < casillas_.GetLength(0))
                {
                    neighbours.Add(casillas_[0, checkCol]);
                }
            }

            for (int c = -1; c <= 1; c++)
            {
                if (c == 0) continue;

                int checkRow = Mathf.RoundToInt(cell.pos.GetRow() + c);

                if (checkRow >= 0 && checkRow < casillas_.GetLength(1))
                {
                    neighbours.Add(casillas_[checkRow, 0]);
                }
            }
            return neighbours;
        }

        //--------------------------------Privates--------------------

        // Inicializacion del Tanke
        private void InitTank() {
            //Destroy(casillas_[0, 0].gameObject);

            Tank t = Instantiate(tankPrefab,
                           new Vector3(-((casillas_.GetLength(1) / 2.0f) * POSITION_FACTOR_C - (POSITION_FACTOR_C / 2.0f)) + 0 * POSITION_FACTOR_C,
                                        1,
                                        (casillas_.GetLength(0) / 2.0f) * POSITION_FACTOR_R - (POSITION_FACTOR_R / 2.0f) - 0 * POSITION_FACTOR_R),
                           Quaternion.identity);

            t.pos = new Position(0, 0);

            t.Init(this, 5);

            setTank(t);
        }

        // Destruccion de todas las casillas del tablero
        private void DestroyCasillas() {
            if (casillas_ == null) throw new InvalidOperationException("This object has not been initialized");

            var rows = casillas_.GetLength(0);
            var columns = casillas_.GetLength(1);

            for (var r = 0u; r < rows; r++)
            {
                for (var c = 0u; c < columns; c++)
                {
                    if (casillas_[r, c] != null)
                        Destroy(casillas_[r, c].gameObject);
                }
            }
        }

        // Generador de casillas 
        private void GenerateCasillas(Map m)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));

            var rows = casillas_.GetLength(0);
            var cols = casillas_.GetLength(1);

            for (var r = 0u; r < rows; r++) {
                for (var c = 0u; c < cols; c++) {
                    //Creamos la casilla en una row y col del vector de casillas
                    Casilla cel = casillas_[r, c];

                    // Le damos esa row y col como posicion
                    Position pos = new Position(r, c);

                    uint value = m.GetValue(pos);

                    if (cel == null) {
                        switch (value) {
                            //Suelo
                            case 0:
                                cel = Instantiate(sueloPrefab,
                            new Vector3(-((casillas_.GetLength(1) / 2.0f) * POSITION_FACTOR_C - (POSITION_FACTOR_C / 2.0f)) + c * POSITION_FACTOR_C,
                                         0,
                                         (casillas_.GetLength(0) / 2.0f) * POSITION_FACTOR_R - (POSITION_FACTOR_R / 2.0f) - r * POSITION_FACTOR_R),
                            Quaternion.identity);
                                break;
                            //Agua
                            case 1:
                                cel = Instantiate(aguaPrefab,
                            new Vector3(-((casillas_.GetLength(1) / 2.0f) * POSITION_FACTOR_C - (POSITION_FACTOR_C / 2.0f)) + c * POSITION_FACTOR_C,
                                         0,
                                         (casillas_.GetLength(0) / 2.0f) * POSITION_FACTOR_R - (POSITION_FACTOR_R / 2.0f) - r * POSITION_FACTOR_R),
                            Quaternion.identity);
                                break;
                            //Barro
                            case 2:
                                cel = Instantiate(barroPrefab,
                            new Vector3(-((casillas_.GetLength(1) / 2.0f) * POSITION_FACTOR_C - (POSITION_FACTOR_C / 2.0f)) + c * POSITION_FACTOR_C,
                                         0,
                                         (casillas_.GetLength(0) / 2.0f) * POSITION_FACTOR_R - (POSITION_FACTOR_R / 2.0f) - r * POSITION_FACTOR_R),
                            Quaternion.identity);
                                break;
                            //Muro
                            case 3:
                                cel = Instantiate(muroPrefab,
                            new Vector3(-((casillas_.GetLength(1) / 2.0f) * POSITION_FACTOR_C - (POSITION_FACTOR_C / 2.0f)) + c * POSITION_FACTOR_C,
                                         0,
                                         (casillas_.GetLength(0) / 2.0f) * POSITION_FACTOR_R - (POSITION_FACTOR_R / 2.0f) - r * POSITION_FACTOR_R),
                            Quaternion.identity);
                                break;
                            //Default
                            default:
                                cel = Instantiate(casillaPrefab,
                            new Vector3(-((casillas_.GetLength(1) / 2.0f) * POSITION_FACTOR_C - (POSITION_FACTOR_C / 2.0f)) + c * POSITION_FACTOR_C,
                                         0,
                                         (casillas_.GetLength(0) / 2.0f) * POSITION_FACTOR_R - (POSITION_FACTOR_R / 2.0f) - r * POSITION_FACTOR_R),
                            Quaternion.identity);
                                break;
                        }

                        cel.pos = pos;

                        cel.Init(this, value);

                        casillas_[r, c] = cel;
                    }
                }
            }
        }

        //-----------------Debbuging tools--------------------

        //Te muestra la posicion de cada casilla en el tablero y su tipo
        public void show() {
            for (int r = 0; r < casillas_.GetLength(0); r++) {
                for (int c = 0; c < casillas_.GetLength(1); c++) {
                    Debug.Log(ToString() + "Pos: " + casillas_[r, c].pos + "    Type: " + casillas_[r, c].type_);
                }
            }
        }

        // Te enseña el camino hasta el candy
        private void showPath() {
            for (int i = 0; i < path_.Count; i++) {
                path_[i].GetComponent<MeshRenderer>().material.color = Color.red;
                //Debug.Log(ToString() + "Camino " + i + ": " + path_[i].pos);
            }
        }
    }
}