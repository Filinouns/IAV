namespace Game_Tank {

    using System;
    using System.Collections;
    using UnityEngine;

    public class Map : IDeepCloneable<Map> {
        public uint rows;
        public uint cols;

        enum CType { Suelo, Agua, Barro, Muro, Candy, Tank, Default };

        //const int NumCasillas = 7;
        //private const int square[NumCasillas] = { 0, 1, 2, 3, 4, 5, 6, 7 };
        

        // La matriz de valores (enteros sin signo) 
        // Podría definirse como tipo genérico, SlidingPuzzle<E> para contener otro tipo de valores.
        private uint[,] matrix;

        // Mantiene una referencia actualizada a la posición del premio
        public Position CandyPos { get; private set; }

        private static readonly uint CANDY_VALUE = 0u;
        private static readonly uint DEFAULT_ROWS = 5u;
        private static readonly uint DEFAULT_COLUMNS = 5u;

        private static readonly int WallChance = 2;
        private static readonly int WaterChance = 3;
        private static readonly int MudChance = 2;

        // Construye la matriz de dimensiones (5) filas por (5) columnas por defecto
        public Map() : this(DEFAULT_ROWS, DEFAULT_COLUMNS)
        {
        }

        // Construye la matriz de dimensiones (rows) por (columns)
        // Como mínimo el puzle debe ser de 2x2
        public Map(uint rows, uint columns)
        {
            if (rows <= 1) throw new ArgumentException(string.Format("{0} is not a valid rows value", rows), "rows");
            if (columns <= 1) throw new ArgumentException(string.Format("{0} is not a valid columns value", columns), "columns");

            this.Init(rows, columns);
        }

        // Constructor de copia, en realidad sirve igual que clonar el puzle
        public Map(Map puzzle)
        {
            if (puzzle == null) throw new ArgumentNullException(nameof(puzzle));

            this.rows = puzzle.rows;
            this.cols = puzzle.cols;

            // Como es de tipos simples, podría aprovecharse la matriz que ya estuviese creada, pero por ahora no lo hacemos
            matrix = new uint[rows, cols];
            for (var r = 0u; r < rows; r++)
                for (var c = 0u; c < cols; c++)
                    matrix[r, c] = puzzle.matrix[r, c];

            // Si la posición del hueco estuviese mal indicada en el otro puzzle se produciría una excepción
            if (puzzle.matrix[puzzle.CandyPos.GetRow(), puzzle.CandyPos.GetColumn()] != CANDY_VALUE)
                throw new ArgumentException(string.Format("{0} is not a valid rows value", rows), "rows");

            CandyPos = puzzle.CandyPos;
        }

        // Devuelve este objeto clonado a nivel profundo
        public Map DeepClone()
        {
            // Uso el constructor de copia para generar un clon
            return new Map(this);
        }

        // Devuelve este objeto de tipo Map clonado a nivel profundo 
        object IDeepCloneable.DeepClone()
        {
            return this.DeepClone();
        }

        // Inicializa o reinicia el puzle, con los valores por defecto (S es el valor que representa al hueco)
        // Como mínimo el puzle debe ser de 2x2
        public void Init(uint rows, uint columns)
        {
            if (rows <= 1) throw new ArgumentException(string.Format("{x<1} is not a valid rows value", rows), "rows");
            if (columns <= 1) throw new ArgumentException(string.Format("{x<1} is not a valid columns value", columns), "columns");

            this.rows = rows;
            this.cols = columns;

            // Podría aprovecharse la matriz que ya estuviese creada, pero por ahora no lo hacemos
            matrix = new uint[rows, columns];

            for (var r = 0u; r < rows; r++) {
                for (var c = 0u; c < columns; c++) {
                    //matrix[r, c] = GetDefaultValue(r, c);
                    matrix[r, c] = GenerateRandSeed(); //Asigna los tipos a las casillas
                    if (matrix[r, c] == CANDY_VALUE)
                        CandyPos = new Position(r, c);
                }

            }
            Console.Write(ToString());
        }

        // Un random para seleccionar el tipo de superficie que sera
        // 0 -> Suelo
        // 1 -> Agua
        // 2 -> Barro
        // 3 -> Muro
        // 4 -> Candy
        // 5 -> Tank
        // 6 -> Default
        // 7 -> flechas
        // Los randoms estan bastante inventados, se puede juguetear con ellos :)
        private uint GenerateRandSeed() {
            int type = 6; //Casilla default (desactivada)

            if (UnityEngine.Random.Range(0, 10) >= 1)    //Transparente o no?
            {
                if (UnityEngine.Random.Range(0, 11) > WallChance)   //Suelo o no?
                {
                    if (UnityEngine.Random.Range(0, 10) > (WaterChance + MudChance - 2))    // Normal o cacota?
                    {
                        type = 0; //Suelo
                    }
                    else if (UnityEngine.Random.Range(0, 6) > WaterChance)  // Cacota :(, Agua almenos?
                    {
                        type = 1; //Agua
                    }
                    else {  //Caguen los randoms...
                        type = 2; //Barro
                    }
                }
                else
                {
                    type = 3; //Muro
                }
            } else {
                type = 6;   // default
            }
            
            uint i = (uint)type;

            return i;
        }

        // Devuelve el valor contenido (uint) en una determinada posición
        // Si no hay ningún valor, se devolverá nulo
        public uint GetValue(Position position)
        {
            if (position == null) throw new ArgumentNullException(nameof(position));
            if (position.GetRow() >= rows) throw new ArgumentException(string.Format("{0} is not a valid row for this matrix", position.GetRow()), "row");
            if (position.GetColumn() >= cols) throw new ArgumentException(string.Format("{0} is not a valid column for this matrix", position.GetColumn()), "column");

            return matrix[position.GetRow(), position.GetColumn()];
        }

        // Compara este puzle con otro objeto y dice si son iguales
        // Sobreescribe la función Equals de object
        public override bool Equals(object o)
        {
            return Equals(o as Map);
        }

        // Compara este puzle con otro y dice si sus configuraciones son iguales
        public bool Equals(Map puzzle)
        {
            if (puzzle == null || puzzle.rows != rows || puzzle.cols != cols)
            {
                return false;
            }

            // Recorrer todos los elementos de ambas matrices
            for (uint r = 0; r < rows; r++)
                for (uint c = 0; c < cols; c++)
                    if (!matrix[r, c].Equals(puzzle.matrix[r, c]))
                        return false;

            return true;
        }

        // Devuelve código hash del puzle (para optimizar el acceso en colecciones y así)
        public override int GetHashCode()
        {
            int result = 17;

            for (uint r = 0; r < rows; r++)
                for (uint c = 0; c < cols; c++)
                    result = 37 * result + Convert.ToInt32(matrix[r, c]);

            return result;
        }

        // Cadena de texto representativa (dibujar una matriz de posiciones separadas por espacios, con \n al final de cada columna o algo así
        public override string ToString()
        {
            return "Puzzle{" + string.Join(",", matrix) + "}"; //Join en realidad sólo funciona con matrices de string, voy a tener que hacer el bucle
        }
    }
}