namespace Game_Tank {

    using System;
    using UnityEngine;

    //using Model;

    public class GameManager : MonoBehaviour {

        //Tablero
        public Tablero board;

        //Objetos de la pantalla
        //public GameObject infoPanel_;
        //public Text timeNumber_;
        //public Text numSteps_;

        //Por si quieres cambiar los valores de tamaño del mapa desde el juego
        //public InputField rowsIn;
        //public InputField colsIn;

        public uint rows_ = 4;
        public uint cols_ = 4;

        //El modelo del mapa
        private Map m_;

        //Resolutor del mapa
        //private PathFinding solver_;
        private double time_ = 0.0d; // in seconds
        private uint steps_ = 0;

        private System.Random rand;

        // Start is called before the first frame update
        void Start() {
            rand = new System.Random();

            Init(rows_, cols_);
        }

        private void Init(uint rows, uint cols)
        {
            if (board == null) throw new InvalidOperationException("The board reference is null");
            /*
            if (infoPanel_ == null) throw new InvalidOperationException("The infoPanel reference is null");
            if (timeNumber_ == null) throw new InvalidOperationException("The timeNumber reference is null");
            if (numSteps_ == null) throw new InvalidOperationException("The stepsNumber reference is null");
            if (rowsIn == null) throw new InvalidOperationException("The rowsInputText reference is null");
            if (colsIn == null) throw new InvalidOperationException("The columnsInputText reference is null");
            */
            rows_ = rows;
            cols_ = cols;

            //rowsIn.text = rows_.ToString();
            //colsIn.text = cols_.ToString();


            //Crear el mapa
            m_ = new Map(rows_, cols_);

            //Iniciar el tablero de bloques
            board.Init(this, m_);

            ResetInfo();

            //UpdateInfo();

            //------Pruebas--------
            board.GiveCandy(7, 5);

            //board.show();
        }

        public void ResetInfo() {
            time_ = 0.0d;
            steps_ = 0;
        }

        // Update is called once per frame
        void Update() {
            //Actualizar los steps para que deje de andar
            //steps_ = Mathf. board.getTank().getSteps();
        }
    }
}