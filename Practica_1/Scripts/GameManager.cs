namespace Game_Tank {

    using System;
    using UnityEngine;
    using UnityEngine.UI;

    //using Model;

    public class GameManager : MonoBehaviour {

        //Tablero
        public Tablero board;

        //Objetos de la pantalla
        //public GameObject infoPanel_;
        //public Text timeNumber_;
        //public Text numSteps_;

        //Texto para informar al usuario del modo
        public Text SearchModeText_;
        public Text PasosText_;
       

        private int sMode_;

        //Para cambiar los valores de tamaño del mapa desde el juego
        public InputField rowsIn;
        public InputField colsIn;

        public uint rows_ = 4;
        public uint cols_ = 4;

        public void setRows(uint r) { rows_ = r; }
        public void setCols(uint c) { cols_ = c; }

        //El modelo del mapa
        private Map m_;

        private double time_ = 0.0d; // in seconds
        private uint steps_ = 0;

        private System.Random rand;

        // Start is called before the first frame update
        void Start() {
            rand = new System.Random();

            Init(rows_, cols_);
        }

        private void Init(uint rows, uint cols) {
            if (board == null) throw new InvalidOperationException("The board reference is null");

            rows_ = rows;
            cols_ = cols;

            rowsIn.text = rows_.ToString();
            colsIn.text = cols_.ToString();

            sMode_ = 0;
            SearchModeText_.text = "SearchMode: " + sMode_;

            //Crear el mapa
            m_ = new Map(rows_, cols_);

            //Iniciar el tablero de bloques
            board.Init(this, m_);

            ResetInfo();

            //------Pruebas--------
            //board.GiveCandy(20, 20);
            //board.show();
        }

        public void ResetInfo() {
            time_ = 0.0d;
            steps_ = 0;
            sMode_ = 0;
        }

        // Update is called once per frame
        void Update() {
            UpdateInfo();
        }

        private void UpdateInfo() {
            steps_ = (uint)board.getTank().getSteps();
            SearchModeText_.text = "SearchMode: " + sMode_;
            PasosText_.text = "Pasos: " + steps_;

            board.getTank().setMode(sMode_);
        }

        public void onReset() {
            if (colsIn.text != null && rowsIn.text != null) {
                cols_ = Convert.ToUInt32(colsIn.text);
                rows_ = Convert.ToUInt32(rowsIn.text);
            }

            m_ = new Map(rows_, cols_);
            board.Reset(this, m_);
            ResetInfo();
        }

        public void changeSearchMode() {
            if (sMode_ == 2) sMode_ = 0;
            else sMode_++;

            board.getTank().setMode(sMode_);
            board.setPath(null);
            board.getTank().setObjetive(false);
        }
    }
}