namespace Game_Tank {

    using System;
    using UnityEngine;

    public class Casilla : MonoBehaviour, IHeapItem<Casilla> {

        public int gCost; // Distancia a la casilla inicial
        public int hCost; // Distancia al objetivo
        public int totalCost;

        public int penalty;

        public int fCost {
            get {
                return gCost + hCost + penalty * 10;
            }
        }

        public void Update()
        {
            totalCost = fCost;
        }

        public int HeapIndex {
            // Lo comentado provoca stackOverflow porque llama todo el rato al get
            /*get {
                return HeapIndex;
            }
            set {
                HeapIndex = value;
            }*/
            get;
            set;
        }

        public int CompareTo(Casilla nodeToCompare) {
            int compare = fCost.CompareTo(nodeToCompare.fCost);
            if (compare == 0) {
                compare = hCost.CompareTo(nodeToCompare.hCost);
            }
            return -compare;
        }

        //Casilla a la que apunta en el path
        [HideInInspector] public Casilla parent;

        //El tablero de casillas
        protected Tablero board_;

        [HideInInspector] public Position pos;

        [HideInInspector] public bool candy_ = false;
        [HideInInspector] public uint type_;

        public void Init(Tablero board, uint t) {
            if (board == null) throw new ArgumentNullException(nameof(board));

            board_ = board;
            type_ = t;

            this.gameObject.SetActive(true);
            /*if (type_ == 6) {   //Desactivar algunas (las default)
                this.gameObject.SetActive(false);
            }
            else {
                this.gameObject.SetActive(true);
            }*/
        }

        public void OnMouseUpAsButton() {
            if (board_ == null) throw new InvalidOperationException("This object has not been initialized");

            if (type_ != 5) board_.changeCasilla(this);
            else board_.activateTank();
        }
    }
}