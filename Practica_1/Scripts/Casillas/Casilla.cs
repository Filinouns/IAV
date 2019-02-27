namespace Game_Tank {  

    using System;

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Casilla : MonoBehaviour {

        public int gCost;
        public int hCost;

        public int fCost {
            get {
                return gCost + hCost;
            }
        }

        public Casilla parent;

        //El tablero de casillas
        protected Tablero board_;

        public Position pos;

        public bool candy_ = false;
        public uint type_;

        public void Init(Tablero board, uint t) {
            if (board == null) throw new ArgumentNullException(nameof(board));

            board_ = board;
            type_ = t;

            if (type_ == 6) {   //Desactivar algunas (las default)
                this.gameObject.SetActive(false);
            }
            else {
                this.gameObject.SetActive(true);
            }
        }

        public void OnMouseUpAsButton()
        {
            if (board_ == null) throw new InvalidOperationException("This object has not been initialized");

            //board_.GiveCandy(this.board_.pos)
            //Quitar el anterior caramelo
            //board_.quitarCarameloAnterior();

            Debug.Log(ToString() + "Tienes un caramelo ahi");
            //return false;
        }
    }
}