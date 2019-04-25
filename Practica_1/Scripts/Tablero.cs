namespace Game_Tank {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using UnityEngine;

    public class Tablero : MonoBehaviour {

        private List<Casilla> path_;
        private List<Casilla> flechas_ = new List<Casilla>();

        [HideInInspector] public float cost;

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
        public Casilla banderaPrefab;
        public Casilla flechaPrefab;

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
            actualizarRuta();
        }

        // ----------------------------Public------------------------

        public void Init(GameManager gm, Map m) {
            if (gm == null) throw new ArgumentNullException(nameof(gm));
            if (m == null) throw new ArgumentNullException(nameof(m));

            gm_ = gm;

            // Inicializamos el vector de casillas
            casillas_ = new Casilla[m.rows, m.cols];
            transform.localScale = new Vector3(SCALE_FACTOR_C * casillas_.GetLength(1), transform.localScale.y, SCALE_FACTOR_R * casillas_.GetLength(0));
            

            rows_ = checked((int)m.rows);
            cols_ = checked((int)m.cols);

            GenerateCasillas(m);

            InitTank();
        }

        

        public void Reset(GameManager gm, Map m) {
            if (gm == null) throw new ArgumentNullException(nameof(gm));
            if (m == null) throw new ArgumentNullException(nameof(m));

            gm_ = gm;

            

            DestroyCasillas();
            path.setWorking(false);
            destroyFlechas();
            path_ = null;

            casillas_ = new Casilla[m.rows, m.cols];
            transform.localScale = new Vector3(SCALE_FACTOR_C * casillas_.GetLength(1), transform.localScale.y, SCALE_FACTOR_R * casillas_.GetLength(0));

            rows_ = checked((int) m.rows);
            cols_ = checked((int) m.cols);

            GenerateCasillas(m);

            ResetTank();
        }

        private void destroyFlechas()
        {
            if (flechas_ != null)
            {
                for (int i = 0; i < flechas_.Count; i++)
                {
                    Destroy(flechas_[i].gameObject);
                }
                flechas_ = new List<Casilla>();
            }
        }

        //Seleccionar una casilla con Candy (Solo se usa en el init de game manager para colocar la primera casilla)
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

        // Inicializacion del Tanke
        private void InitTank() {
            Tank t = Instantiate(tankPrefab,
                           new Vector3(-((casillas_.GetLength(1) / 2.0f) * POSITION_FACTOR_C - (POSITION_FACTOR_C / 2.0f)) + 0 * POSITION_FACTOR_C,
                                        1,
                                        (casillas_.GetLength(0) / 2.0f) * POSITION_FACTOR_R - (POSITION_FACTOR_R / 2.0f) - 0 * POSITION_FACTOR_R),
                           Quaternion.identity);

            t.pos = new Position(0, 0);

            t.Init(this, 5);

            setTank(t);
        }

        // Elimina el tanque y crea uno nuevo
        private void ResetTank() {
            Destroy(tank_.gameObject);
            InitTank();
        }

        //Actualiza la ruta del tank
        private void actualizarRuta() {
            if (path_ != null) {
                if (!Moving()) {
                    tank_.setObjetive(false);
                    path.setWorking(false);
                    path_ = null;
                }
                else
                {
                    Debug.Log(ToString() + "Coste: " + path.cost);
                    if (path_.Contains(candy_)) {
                        tank_.setObjetive(true);
                        showPath();
                    }
                }
            }
        }

        //Comprobacion para terminar el movimiento
        private bool Moving() {
            bool move = true;

            if (tank_.getSteps() >= path_.Count) {
                move = false;
            }
           
            return move;
        }

        //Nos devuelve la posicion del siguiente paso del camino del tank
        public Casilla MoveTank(int steps)
        {
            if (path_ == null) throw new InvalidOperationException("This object has not been initialized");

            if (path_ != null)
            {
                if (steps > path_.Count - 1)
                {
                    return null;
                }
                else
                {
                    cost += path_[steps].penalty;
                    return path_[steps];
                }
            }
            return null;
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

        // Busca los 4 vecinos de una casilla, en horizontal y vertical
        public List<Casilla> Get4Neighbours(Casilla cell)
        {
            List<Casilla> neighbours = new List<Casilla>();

            for (int r = -1; r <= 1; r++)
            {
                for (int c = -1; c <= 1; c++)
                {
                    if (r == 0 && c == 0) continue;
                    if (r== 0 || c == 0) { //Para no coger las diagonales
                        int checkCol = Mathf.RoundToInt(cell.pos.GetColumn() + r);
                        int checkRow = Mathf.RoundToInt(cell.pos.GetRow() + c);

                        if (checkCol >= 0 && checkCol < casillas_.GetLength(0) && checkRow >= 0 && checkRow < casillas_.GetLength(1))
                        {
                            neighbours.Add(casillas_[checkRow, checkCol]);
                        }
                    }
                }
            }
            return neighbours;
        }

        // Cambia el estado del tank
        public void activateTank() {
            if (tank_.selected) tank_.selected = false;
            else tank_.selected = true;
        }

        // Elimina el path y cambia el estado del tanque a "Sin objetivo"
        private void resetPath() {
            destroyFlechas();
            path.cost = 0;
            path_ = null;
            path.setWorking(false);
            tank_.setObjetive(false);
        }

        //-------------------------------------------------Interacciones con el tablero---------------------------------

        // Cambia un tipo de casilla por otra
        public void changeCasilla(Casilla c) {
            Casilla cel;
            if (!tank_.selected) {
                if (tank_.pos != c.pos) { // Si la posicion de la casilla y el tanque no es la misma
                    switch (c.type_)
                    {
                        case 0: // El suelo pasa a ser agua
                            cel = Instantiate(aguaPrefab,
                            new Vector3(-((casillas_.GetLength(1) / 2.0f) * POSITION_FACTOR_C - (POSITION_FACTOR_C / 2.0f)) + c.pos.GetColumn() * POSITION_FACTOR_C,
                                         0,
                                         (casillas_.GetLength(0) / 2.0f) * POSITION_FACTOR_R - (POSITION_FACTOR_R / 2.0f) - c.pos.GetRow() * POSITION_FACTOR_R),
                            Quaternion.identity);

                            cel.pos = c.pos;
                            cel.Init(this, 1);
                            Destroy(casillas_[c.pos.GetRow(), c.pos.GetColumn()].gameObject); //Borramos la casilla a cambiar
                            casillas_[cel.pos.GetRow(), cel.pos.GetColumn()] = cel; //Asignamos la nueva casilla
                            break;
                        case 1: // El agua pasa a ser barro
                            cel = Instantiate(barroPrefab,
                            new Vector3(-((casillas_.GetLength(1) / 2.0f) * POSITION_FACTOR_C - (POSITION_FACTOR_C / 2.0f)) + c.pos.GetColumn() * POSITION_FACTOR_C,
                                         0,
                                         (casillas_.GetLength(0) / 2.0f) * POSITION_FACTOR_R - (POSITION_FACTOR_R / 2.0f) - c.pos.GetRow() * POSITION_FACTOR_R),
                            Quaternion.identity);
                            cel.pos = c.pos;
                            cel.Init(this, 2);
                            Destroy(casillas_[c.pos.GetRow(), c.pos.GetColumn()].gameObject); //Borramos la casilla a cambiar
                            casillas_[cel.pos.GetRow(), cel.pos.GetColumn()] = cel; //Asignamos la nueva casilla
                            break;
                        case 2: // El barro pasa a ser muro
                            cel = Instantiate(muroPrefab,
                            new Vector3(-((casillas_.GetLength(1) / 2.0f) * POSITION_FACTOR_C - (POSITION_FACTOR_C / 2.0f)) + c.pos.GetColumn() * POSITION_FACTOR_C,
                                         0,
                                         (casillas_.GetLength(0) / 2.0f) * POSITION_FACTOR_R - (POSITION_FACTOR_R / 2.0f) - c.pos.GetRow() * POSITION_FACTOR_R),
                            Quaternion.identity);
                            cel.pos = c.pos;
                            cel.Init(this, 3);
                            Destroy(casillas_[c.pos.GetRow(), c.pos.GetColumn()].gameObject); //Borramos la casilla a cambiar
                            casillas_[cel.pos.GetRow(), cel.pos.GetColumn()] = cel; //Asignamos la nueva casilla
                            break;
                        case 3: // El muro pasa a ser suelo
                            cel = Instantiate(sueloPrefab,
                            new Vector3(-((casillas_.GetLength(1) / 2.0f) * POSITION_FACTOR_C - (POSITION_FACTOR_C / 2.0f)) + c.pos.GetColumn() * POSITION_FACTOR_C,
                                         0,
                                         (casillas_.GetLength(0) / 2.0f) * POSITION_FACTOR_R - (POSITION_FACTOR_R / 2.0f) - c.pos.GetRow() * POSITION_FACTOR_R),
                            Quaternion.identity);
                            cel.pos = c.pos;
                            cel.Init(this, 0);
                            Destroy(casillas_[c.pos.GetRow(), c.pos.GetColumn()].gameObject); //Borramos la casilla a cambiar
                            casillas_[cel.pos.GetRow(), cel.pos.GetColumn()] = cel; //Asignamos la nueva casilla
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                resetPath();
                switch (c.type_) {
                    case 0: // Nuevo candy!
                        createCandy(c);
                        break;
                    case 1: // Nuevo candy!
                        createCandy(c);
                        break;
                    case 2: // Nuevo candy!
                        createCandy(c);
                        break;
                    default:
                        break;
                }
            }
        }

        //--------------------------------Privates--------------------

        //Crea un candy en la posicion de la casilla que le pasas
        private void createCandy(Casilla c) {
            if (candy_ != null) deleteLastCandy();

            Casilla cel = Instantiate(banderaPrefab,
                            new Vector3(-((casillas_.GetLength(1) / 2.0f) * POSITION_FACTOR_C - (POSITION_FACTOR_C / 2.0f)) + c.pos.GetColumn() * POSITION_FACTOR_C,
                                         0,
                                         (casillas_.GetLength(0) / 2.0f) * POSITION_FACTOR_R - (POSITION_FACTOR_R / 2.0f) - c.pos.GetRow() * POSITION_FACTOR_R),
                            Quaternion.identity.normalized);
            cel.transform.Rotate(0, 180, 0, Space.Self);
            cel.pos = c.pos;
            cel.Init(this, 4);
            cel.candy_ = true;
            setCandy(cel);

            Destroy(casillas_[c.pos.GetRow(), c.pos.GetColumn()].gameObject); //Borramos la casilla a cambiar
            casillas_[cel.pos.GetRow(), cel.pos.GetColumn()] = cel; //Asignamos la nueva casilla
        }

        //Elimina el candy actual y lo sustituye por una casilla pisable cualquiera
        private void deleteLastCandy() {
            Casilla cel = null;
            switch (UnityEngine.Random.Range(0, 3)) {
                case 0: //Creamos un suelo
                    cel = Instantiate(sueloPrefab,
                            new Vector3(-((casillas_.GetLength(1) / 2.0f) * POSITION_FACTOR_C - (POSITION_FACTOR_C / 2.0f)) + candy_.pos.GetColumn() * POSITION_FACTOR_C,
                                         0,
                                         (casillas_.GetLength(0) / 2.0f) * POSITION_FACTOR_R - (POSITION_FACTOR_R / 2.0f) - candy_.pos.GetRow() * POSITION_FACTOR_R),
                            Quaternion.identity);
                    cel.pos = candy_.pos;
                    cel.Init(this, 0);
                    break;
                case 1: //Creamos un agua
                    cel = Instantiate(aguaPrefab,
                            new Vector3(-((casillas_.GetLength(1) / 2.0f) * POSITION_FACTOR_C - (POSITION_FACTOR_C / 2.0f)) + candy_.pos.GetColumn() * POSITION_FACTOR_C,
                                         0,
                                         (casillas_.GetLength(0) / 2.0f) * POSITION_FACTOR_R - (POSITION_FACTOR_R / 2.0f) - candy_.pos.GetRow() * POSITION_FACTOR_R),
                            Quaternion.identity);
                    cel.pos = candy_.pos;
                    cel.Init(this, 1);
                    break;
                case 2: //Creamos un barro
                    cel = Instantiate(barroPrefab,
                            new Vector3(-((casillas_.GetLength(1) / 2.0f) * POSITION_FACTOR_C - (POSITION_FACTOR_C / 2.0f)) + candy_.pos.GetColumn() * POSITION_FACTOR_C,
                                         0,
                                         (casillas_.GetLength(0) / 2.0f) * POSITION_FACTOR_R - (POSITION_FACTOR_R / 2.0f) - candy_.pos.GetRow() * POSITION_FACTOR_R),
                            Quaternion.identity);
                    cel.pos = candy_.pos;
                    cel.Init(this, 2);
                    break;
                default:
                    break;
            }
            if (cel != null) {
                Destroy(casillas_[candy_.pos.GetRow(), candy_.pos.GetColumn()].gameObject);
                casillas_[cel.pos.GetRow(), cel.pos.GetColumn()] = cel;
            }
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
                                cel = Instantiate(sueloPrefab,
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
        public void showPath() {
                for (int i = 0; i < path_.Count - 1; i++) {
                Vector3 holi = path_[i].transform.position - path_[i + 1].transform.position;
               
                 Casilla cel = Instantiate(flechaPrefab, new Vector3(path_[i].transform.position.x, (float)0.3, path_[i].transform.position.z),
                        Quaternion.identity.normalized);

                    if (holi.x == 0 && holi.z > 0)
                    {
                        cel.transform.Rotate(new Vector3(0, 0, 0), Space.Self);
                    }
                    else if (holi.x < 0 && holi.z == 0)
                    {
                        cel.transform.Rotate(new Vector3(0, -90, 0), Space.Self);
                    }
                    else if (holi.x == 0 && holi.z < 0)
                    {
                        cel.transform.Rotate(new Vector3(0, 180, 0), Space.Self);
                    }
                    else if (holi.x > 0 && holi.z == 0)
                    {
                        cel.transform.Rotate(new Vector3(0, 90, 0), Space.Self);
                    }
                    cel.Init(this, 7);
                    flechas_.Add(cel);
            }
        }
    }
}