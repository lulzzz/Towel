﻿using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using Towel.Measurements;

namespace Towel.Mathematics
{
    /// <summary>A matrix of arbitrary dimensions implemented as a flattened array.</summary>
    /// <typeparam name="T">The numeric type of this Matrix.</typeparam>
    [DebuggerDisplay("{" + nameof(DebuggerString) + "}")]
    [Serializable]
    public class Matrix<T>
    {
        internal readonly T[] _matrix;
        internal int _rows;
        internal int _columns;

        #region Properties

        internal int Length { get { return this._matrix.Length; } }
        /// <summary>The number of rows in the matrix.</summary>
        public int Rows { get { return this._rows; } }
        /// <summary>The number of columns in the matrix.</summary>
        public int Columns { get { return this._columns; } }
        /// <summary>Determines if the matrix is square.</summary>
        public bool IsSquare { get { return this._rows == this._columns; } }
        /// <summary>Determines if the matrix is a vector.</summary>
        public bool IsVector { get { return this._columns == 1; } }
        /// <summary>Determines if the matrix is a 2 component vector.</summary>
        public bool Is2x1 { get { return this._rows == 2 && this._columns == 1; } }
        /// <summary>Determines if the matrix is a 3 component vector.</summary>
        public bool Is3x1 { get { return this._rows == 3 && this._columns == 1; } }
        /// <summary>Determines if the matrix is a 4 component vector.</summary>
        public bool Is4x1 { get { return this._rows == 4 && this._columns == 1; } }
        /// <summary>Determines if the matrix is a 2 square matrix.</summary>
        public bool Is2x2 { get { return this._rows == 2 && this._columns == 2; } }
        /// <summary>Determines if the matrix is a 3 square matrix.</summary>
        public bool Is3x3 { get { return this._rows == 3 && this._columns == 3; } }
        /// <summary>Determines if the matrix is a 4 square matrix.</summary>
        public bool Is4x4 { get { return this._rows == 4 && this._columns == 4; } }

        /// <summary>Standard row-major matrix indexing.</summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>The value at the given indeces.</returns>
        public T this[int row, int column]
        {
            get
            {
                if (row < 0 || row > this.Rows)
                {
                    throw new ArgumentOutOfRangeException(nameof(row), row, "!(" + nameof(row) + " >= 0) || !(" + nameof(row) + " < " + nameof(Rows) + ")");
                }
                if (column < 0 || column > this.Columns)
                {
                    throw new ArgumentOutOfRangeException(nameof(column), row, "!(" + nameof(column) + " >= 0) || !(" + nameof(column) + " < " + nameof(Columns) + ")");
                }
                return Get(row, column);
            }
            set
            {
                if (row < 0 || row > this.Rows)
                {
                    throw new ArgumentOutOfRangeException(nameof(row), row, "!(" + nameof(row) + " >= 0) || !(" + nameof(row) + " < " + nameof(Rows) + ")");
                }
                if (column < 0 || column > this.Columns)
                {
                    throw new ArgumentOutOfRangeException(nameof(column), row, "!(" + nameof(column) + " >= 0) || !(" + nameof(column) + " < " + nameof(Columns) + ")");
                }
                this.Set(row, column, value);
            }
        }

        #endregion

        #region Debugger Properties

        internal string DebuggerString
        {
            get
            {
                int Rows = this.Rows;
                int Columns = this.Columns;
                if (Rows < 5 && Columns < 5)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine("[ ");
                    for (int i = 0; i < Rows; i++)
                    {
                        stringBuilder.Append("[ ");
                        for (int j = 0; j < Columns; j++)
                        {
                            stringBuilder.Append(this[i, j]);
                            if (j < Columns - 1)
                            {
                                stringBuilder.Append(", ");
                            }
                        }
                        stringBuilder.AppendLine("[ ");
                    }
                    stringBuilder.Append(" ]");
                    return stringBuilder.ToString();
                }
                return this.ToString();
            }
        }

        #endregion

        #region Constructors

        internal Matrix(int rows, int columns, int length)
        {
            this._matrix = new T[length];
            this._rows = rows;
            this._columns = columns;
        }

        /// <summary>Constructs a new default matrix of the given dimensions.</summary>
        /// <param name="rows">The number of row dimensions.</param>
        /// <param name="columns">The number of column dimensions.</param>
        public Matrix(int rows, int columns)
        {
            if (rows < 1)
            {
                throw new ArgumentOutOfRangeException("rows", rows, "!(rows > 0)");
            }
            if (columns < 1)
            {
                throw new ArgumentOutOfRangeException("columns", columns, "!(columns > 0)");
            }
            this._matrix = new T[rows * columns];
            this._rows = rows;
            this._columns = columns;
        }

        /// <summary>Constructs a new matrix and initializes it via function.</summary>
        /// <param name="rows">The number of rows to construct.</param>
        /// <param name="columns">The number of columns to construct.</param>
        /// <param name="function">The initialization function.</param>
        public Matrix(int rows, int columns, Func<int, int, T> function) : this(rows, columns)
        {
            Fill(this, function);
        }

        /// <summary>
        /// Creates a new matrix using ROW MAJOR ORDER. The data will be referenced to, so 
        /// changes to it will modify the constructed matrix.
        /// </summary>
        /// <param name="rows">The number of rows to construct.</param>
        /// <param name="columns">The number of columns to construct.</param>
        /// <param name="data">The data of the matrix in ROW MAJOR ORDER.</param>
        internal Matrix(int rows, int columns, params T[] data) : this(rows, columns)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (data.Length != rows * columns)
            {
                throw new ArgumentException("!(" + nameof(rows) + " * " + nameof(columns) + " == " + nameof(data) + "." + nameof(data.Length) + ")");
            }
            this._rows = rows;
            this._columns = columns;
            this._matrix = data;
        }

        private Matrix(Matrix<T> matrix)
        {
            this._rows = matrix._rows;
            this._columns = matrix.Columns;
            this._matrix = (matrix._matrix).Clone() as T[];
        }

        internal Matrix(Vector<T> vector)
        {
            this._rows = vector.Dimensions;
            this._columns = 1;
            this._matrix = vector._vector.Clone() as T[];
        }

		#endregion

		#region Factories

		/// <summary>Constructs a new zero-matrix of the given dimensions.</summary>
		/// <param name="rows">The number of rows of the matrix.</param>
		/// <param name="columns">The number of columns of the matrix.</param>
		/// <returns>The newly constructed zero-matrix.</returns>
		public static Matrix<T> FactoryZero(int rows, int columns)
		{
            if (rows < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(rows), rows, "!(" + nameof(rows) + " > 0)");
            }
            if (columns < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(columns), columns, "!(" + nameof(columns) + " > 0)");
            }
            return FactoryZeroImplementation(rows, columns);
		}

        internal static Func<int, int, Matrix<T>> FactoryZeroImplementation = (rows, columns) =>
        {
            if (Compute.Equal(default(T), Constant<T>.Zero))
            {
                FactoryZeroImplementation = (ROWS, COLUMNS) => new Matrix<T>(ROWS, COLUMNS);
            }
            else
            {
                FactoryZeroImplementation = (ROWS, COLUMNS) =>
                {
                    Matrix<T> matrix = new Matrix<T>(ROWS, COLUMNS);
                    matrix._matrix.Fill(Constant<T>.Zero);
                    return matrix;
                };
            }
            return FactoryZeroImplementation(rows, columns);
        };

        /// <summary>Constructs a new identity-matrix of the given dimensions.</summary>
        /// <param name="rows">The number of rows of the matrix.</param>
        /// <param name="columns">The number of columns of the matrix.</param>
        /// <returns>The newly constructed identity-matrix.</returns>
        public static Matrix<T> FactoryIdentity(int rows, int columns)
		{
            if (rows < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(rows), rows, "!(" + nameof(rows) + " > 0)");
            }
            if (columns < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(columns), columns, "!(" + nameof(columns) + " > 0)");
            }
            return FactoryIdentityImplementation(rows, columns);
		}

        internal static Func<int, int, Matrix<T>> FactoryIdentityImplementation = (rows, columns) =>
        {
            if (Compute.Equal(default(T), Constant<T>.Zero))
            {
                FactoryIdentityImplementation = (ROWS, COLUMNS) =>
                {
                    Matrix<T> matrix = new Matrix<T>(ROWS, COLUMNS);
                    T[] MATRIX = matrix._matrix;
                    int minimum = Compute.Minimum(rows, columns);
                    for (int i = 1; i < minimum; i++)
                    {
                        MATRIX[i * i] = Constant<T>.One;
                    }
                    return matrix;
                };
            }
            else
            {
                FactoryIdentityImplementation = (ROWS, COLUMNS) =>
                {
                    Matrix<T> matrix = new Matrix<T>(ROWS, COLUMNS);
                    Fill(matrix, (x, y) => x == y ? Constant<T>.One : Constant<T>.Zero);
                    return matrix;
                };
            }
            return FactoryIdentityImplementation(rows, columns);
        };

        /// <summary>Constructs a new matrix where every entry is the same uniform value.</summary>
        /// <param name="rows">The number of rows of the matrix.</param>
        /// <param name="columns">The number of columns of the matrix.</param>
        /// <param name="value">The value to assign every spot in the matrix.</param>
        /// <returns>The newly constructed matrix filled with the uniform value.</returns>
        public static Matrix<T> FactoryUniform(int rows, int columns, T value)
		{
            if (rows < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(rows), rows, "!(" + nameof(rows) + " > 0)");
            }
            if (columns < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(columns), columns, "!(" + nameof(columns) + " > 0)");
            }
            Matrix<T> matrix = new Matrix<T>(rows, columns);
            matrix._matrix.Fill(value);
			return matrix;
		}

        #endregion

        #region Mathematics

        #region RowMultiplication

        private static void RowMultiplication(Matrix<T> matrix, int row, T scalar)
        {
            int columns = matrix.Columns;
            for (int i = 0; i < columns; i++)
            {
                matrix.Set(row, i, Compute.Multiply(matrix.Get(row, i), scalar));
            }
        }

        #endregion

        #region RowAddition

        private static void RowAddition(Matrix<T> matrix, int target, int second, T scalar)
        {
            int columns = matrix.Columns;
            for (int i = 0; i < columns; i++)
            {
                matrix.Set(target, i, Compute.Add(matrix.Get(target, i), Compute.Multiply(matrix.Get(second, i), scalar)));
                matrix[target, i] = Compute.Add(matrix.Get(target, i), Compute.Multiply(matrix.Get(second, i), scalar));
            }
        }

        #endregion

        #region SwapRows

        private static void SwapRows(Matrix<T> matrix, int row1, int row2)
        {
            int columns = matrix.Columns;
            for (int i = 0; i < columns; i++)
            {
                T temp = matrix.Get(row1, i);
                matrix.Set(row1, i, matrix.Get(row2, i));
                matrix.Set(row2, i, temp);
            }
        }

        #endregion

        #region IsSymetric

        /// <summary>Determines if the matrix is symetric.</summary>
        /// <param name="a">The matrix to determine if symetric.</param>
        /// <returns>True if the matrix is symetric; false if not.</returns>
        public static bool GetIsSymetric(Matrix<T> a)
        {
            if (a is null)
            {
                throw new ArgumentNullException(nameof(a));
            }
            int rows = a._rows;
            if (rows != a._columns)
            {
                return false;
            }
            T[] A = a._matrix;
            for (int row = 0; row < rows; row++)
            {
                for (int column = row + 1; column < rows; column++)
                {
                    if (Compute.NotEqual(A[row * rows + column], A[column * rows + row]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>Determines if the matrix is symetric.</summary>
        /// <returns>True if the matrix is symetric; false if not.</returns>
        public bool IsSymetric
        {
            get
            {
                return GetIsSymetric(this);
            }
        }

        #endregion

        #region Negate

        /// <summary>Negates all the values in a matrix.</summary>
        /// <param name="a">The matrix to have its values negated.</param>
        /// <param name="b">The resulting matrix after the negation.</param>
        private static void Negate(Matrix<T> a, ref Matrix<T> b)
        {
            if (a is null)
            {
                throw new ArgumentNullException(nameof(a));
            }
            int Length = a._matrix.Length;
            T[] A = a._matrix;
            T[] B;
            if (b != null && b._matrix.Length == Length)
            {
                B = b._matrix;
                b._rows = a._rows;
                b._columns = a._columns;
            }
            else
            {
                b = new Matrix<T>(a._rows, a._columns, Length);
                B = b._matrix;
            }
            for (int i = 0; i < Length; i++)
            {
                B[i] = Compute.Negate(A[i]);
            }
        }

        /// <summary>Negates all the values in a matrix.</summary>
		/// <param name="a">The matrix to have its values negated.</param>
		/// <returns>The resulting matrix after the negation.</returns>
		public static Matrix<T> Negate(Matrix<T> a)
        {
            Matrix<T> b = null;
            Negate(a, ref b);
            return b;
        }

        /// <summary>Negates all the values in a matrix.</summary>
        /// <param name="a">The matrix to have its values negated.</param>
        /// <returns>The resulting matrix after the negation.</returns>
        public static Matrix<T> operator -(Matrix<T> a)
        {
            return Negate(a);
        }

        /// <summary>Negates all the values in a matrix.</summary>
        /// <param name="b">The resulting matrix after the negation.</param>
		public void Negate(ref Matrix<T> b)
        {
            Negate(this, ref b);
        }

        /// <summary>Negates all the values in this matrix.</summary>
		/// <returns>The resulting matrix after the negation.</returns>
		public Matrix<T> Negate()
        {
            return -this;
        }

        #endregion

        #region Add

        /// <summary>Does standard addition of two matrices.</summary>
		/// <param name="a">The left matrix of the addition.</param>
		/// <param name="b">The right matrix of the addition.</param>
        /// <param name="c">The resulting matrix after the addition.</param>
        private static void Add(Matrix<T> a, Matrix<T> b, ref Matrix<T> c)
        {
            if (a is null)
            {
                throw new ArgumentNullException(nameof(a));
            }
            if (b is null)
            {
                throw new ArgumentNullException(nameof(b));
            }
            if (a._rows != b._rows || a._columns != b._columns)
            {
                throw new MathematicsException("Arguments invalid !(" +
                    nameof(a) + "." + nameof(a.Rows) + " == " + nameof(b) + "." + nameof(b.Rows) + " && " +
                    nameof(a) + "." + nameof(a.Columns) + " == " + nameof(b) + "." + nameof(b.Columns) + ")");
            }
            int Length = a._matrix.Length;
            T[] A = a._matrix;
            T[] B = b._matrix;
            T[] C;
            if (c != null && c._matrix.Length == Length)
            {
                C = c._matrix;
                c._rows = a._rows;
                c._columns = a._columns;
            }
            else
            {
                c = new Matrix<T>(a._rows, a._columns, Length);
                C = c._matrix;
            }
            for (int i = 0; i < Length; i++)
            {
                C[i] = Compute.Add(A[i], B[i]);
            }
        }

        /// <summary>Does standard addition of two matrices.</summary>
		/// <param name="a">The left matrix of the addition.</param>
		/// <param name="b">The right matrix of the addition.</param>
		/// <returns>The resulting matrix after the addition.</returns>
		public static Matrix<T> Add(Matrix<T> a, Matrix<T> b)
        {
            Matrix<T> c = null;
            Add(a, b, ref c);
            return c;
        }

        /// <summary>Does a standard matrix addition.</summary>
        /// <param name="a">The left matrix of the addition.</param>
        /// <param name="b">The right matrix of the addition.</param>
        /// <returns>The resulting matrix after teh addition.</returns>
        public static Matrix<T> operator +(Matrix<T> a, Matrix<T> b)
        {
            return Add(a, b);
        }

        /// <summary>Does standard addition of two matrices.</summary>
		/// <param name="b">The right matrix of the addition.</param>
        /// <param name="c">The resulting matrix after the addition.</param>
		public void Add(Matrix<T> b, ref Matrix<T> c)
        {
            Add(this, b, ref c);
        }

        /// <summary>Does a standard matrix addition.</summary>
		/// <param name="b">The matrix to add to this matrix.</param>
		/// <returns>The resulting matrix after the addition.</returns>
		public Matrix<T> Add(Matrix<T> b)
        {
            return this + b;
        }
        
        #endregion

        #region Subtract

        /// <summary>Does a standard matrix subtraction.</summary>
		/// <param name="a">The left matrix of the subtraction.</param>
        /// <param name="b">The right matrix of the subtraction.</param>
        /// <param name="c">The resulting matrix after the subtraction.</param>
        private static void Subtract(Matrix<T> a, Matrix<T> b, ref Matrix<T> c)
        {
            if (a is null)
            {
                throw new ArgumentNullException(nameof(a));
            }
            if (b is null)
            {
                throw new ArgumentNullException(nameof(b));
            }
            if (a._rows != b._rows || a._columns != b._columns)
            {
                throw new MathematicsException("Arguments invalid !(" +
                    nameof(a) + "." + nameof(a.Rows) + " == " + nameof(b) + "." + nameof(b.Rows) + " && " +
                    nameof(a) + "." + nameof(a.Columns) + " == " + nameof(b) + "." + nameof(b.Columns) + ")");
            }
            T[] A = a._matrix;
            T[] B = b._matrix;
            int Length = A.Length;
            T[] C;
            if (c != null && c._matrix.Length == Length)
            {
                C = c._matrix;
                c._rows = a._rows;
                c._columns = a._columns;
            }
            else
            {
                c = new Matrix<T>(a._rows, a._columns, Length);
                C = c._matrix;
            }
            for (int i = 0; i < Length; i++)
            {
                C[i] = Compute.Subtract(A[i], B[i]);
            }
        }

        /// <summary>Does a standard matrix subtraction.</summary>
		/// <param name="a">The left matrix of the subtraction.</param>
        /// <param name="b">The right matrix of the subtraction.</param>
		/// <returns>The resulting matrix after the subtraction.</returns>
		public static Matrix<T> Subtract(Matrix<T> a, Matrix<T> b)
        {
            Matrix<T> c = null;
            Subtract(a, b, ref c);
            return c;
        }

        /// <summary>Does a standard matrix subtraction.</summary>
        /// <param name="a">The left matrix of the subtraction.</param>
        /// <param name="b">The right matrix of the subtraction.</param>
        /// <returns>The resulting matrix after the subtraction.</returns>
        public static Matrix<T> operator -(Matrix<T> a, Matrix<T> b)
        {
            return Subtract(a, b);
        }

        /// <summary>Does a standard matrix subtraction.</summary>
        /// <param name="b">The right matrix of the subtraction.</param>
        /// <param name="c">The resulting matrix after the subtraction.</param>
		public void Subtract(Matrix<T> b, ref Matrix<T> c)
        {
            Subtract(this, b, ref c);
        }

        /// <summary>Does a standard matrix subtraction.</summary>
		/// <param name="b">The right matrix of the subtraction.</param>
		/// <returns>The resulting matrix after the subtraction.</returns>
		public Matrix<T> Subtract(Matrix<T> b)
        {
            return this - b;
        }

        #endregion

        #region Multiply (Matrix * Matrix)

        /// <summary>Does a standard (triple for looped) multiplication between matrices.</summary>
		/// <param name="a">The left matrix of the multiplication.</param>
		/// <param name="b">The right matrix of the multiplication.</param>
        /// <param name="c">The resulting matrix of the multiplication.</param>
        private static void Multiply(Matrix<T> a, Matrix<T> b, ref Matrix<T> c)
        {
            if (a is null)
            {
                throw new ArgumentNullException(nameof(a));
            }
            if (b is null)
            {
                throw new ArgumentNullException(nameof(b));
            }
            if (a._columns != b._rows)
            {
                throw new MathematicsException("Arguments invalid !(" +
                    nameof(a) + "." + nameof(a.Columns) + " == " + nameof(b) + "." + nameof(b.Rows) + ")");
            }
            if (object.ReferenceEquals(a, b) && object.ReferenceEquals(a, c))
            {
                Matrix<T> clone = a.Clone();
                a = clone;
                b = clone;
            }
            else if (object.ReferenceEquals(a, c))
            {
                a = a.Clone();
            }
            else if (object.ReferenceEquals(b, c))
            {
                b = b.Clone();
            }
            int c_Rows = a._rows;
            int a_Columns = a._columns;
            int c_Columns = b._columns;
            T[] A = a._matrix;
            T[] B = b._matrix;
            T[] C;
            if (c != null && c._matrix.Length == c_Rows * c_Columns)
            {
                C = c._matrix;
                c._rows = c_Rows;
                c._columns = c_Columns;
            }
            else
            {
                c = new Matrix<T>(c_Rows, c_Columns);
                C = c._matrix;
            }
            for (int i = 0; i < c_Rows; i++)
            {
                int i_times_a_Columns = i * a_Columns;
                int i_times_c_Columns = i * c_Columns;
                for (int j = 0; j < c_Columns; j++)
                {
                    T sum = Constant<T>.Zero;
                    for (int k = 0; k < a_Columns; k++)
                    {
                        sum = MultiplyThenAddImplementation.Function(A[i_times_a_Columns + k], B[k * c_Columns + j], sum);
                    }
                    C[i_times_c_Columns + j] = sum;
                }
            }
        }

        internal static class MultiplyThenAddImplementation
        {
            internal static Func<T, T, T, T> Function = (T a, T b, T c) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(T));
                ParameterExpression B = Expression.Parameter(typeof(T));
                ParameterExpression C = Expression.Parameter(typeof(T));
                Expression BODY = Expression.Add(Expression.Multiply(A, B), C);
                Function = Expression.Lambda<Func<T, T, T, T>>(BODY, A, B, C).Compile();
                return Function(a, b, c);
            };
        }

        /// <summary>Does a standard (triple for looped) multiplication between matrices.</summary>
		/// <param name="a">The left matrix of the multiplication.</param>
		/// <param name="b">The right matrix of the multiplication.</param>
		/// <returns>The resulting matrix of the multiplication.</returns>
		public static Matrix<T> Multiply(Matrix<T> a, Matrix<T> b)
        {
            Matrix<T> c = null;
            Multiply(a, b, ref c);
            return c;
        }

        /// <summary>Does a standard (triple for looped) multiplication between matrices.</summary>
		/// <param name="a">The left matrix of the multiplication.</param>
		/// <param name="b">The right matrix of the multiplication.</param>
		/// <returns>The resulting matrix of the multiplication.</returns>
		public static Matrix<T> operator *(Matrix<T> a, Matrix<T> b)
        {
            return Multiply(a, b);
        }

        /// <summary>Does a standard (triple for looped) multiplication between matrices.</summary>
		/// <param name="b">The right matrix of the multiplication.</param>
        /// <param name="c">The resulting matrix of the multiplication.</param>
        public void Multiply(Matrix<T> b, ref Matrix<T> c)
        {
            Multiply(this, b, ref c);
        }

        /// <summary>Does a standard (triple for looped) multiplication between matrices.</summary>
		/// <param name="b">The right matrix of the multiplication.</param>
		/// <returns>The resulting matrix of the multiplication.</returns>
		public Matrix<T> Multiply(Matrix<T> b)
        {
            return this * b;
        }

        #endregion

        #region Multiply (Matrix * Vector)

        /// <summary>Does a matrix-vector multiplication.</summary>
        /// <param name="a">The left matrix of the multiplication.</param>
        /// <param name="b">The right vector of the multiplication.</param>
        /// <param name="c">The resulting vector of the multiplication.</param>
        private static void Multiply(Matrix<T> a, Vector<T> b, ref Vector<T> c)
        {
            if (a is null)
            {
                throw new ArgumentNullException(nameof(a));
            }
            if (b is null)
            {
                throw new ArgumentNullException(nameof(b));
            }
            int rows = a._rows;
            int columns = a._columns;
            if (columns != b.Dimensions)
            {
                throw new MathematicsException("Arguments invalid !(" +
                    nameof(a) + "." + nameof(a.Columns) + " == " + nameof(b) + "." + nameof(b.Dimensions) + ")");
            }
            T[] A = a._matrix;
            T[] B = b._vector;
            T[] C;
            if (c != null && c.Dimensions == columns)
            {
                C = c._vector;
            }
            else
            {
                c = new Vector<T>(columns);
                C = c._vector;
            }
            for (int i = 0; i < rows; i++)
            {
                int i_times_columns = i * columns;
                T sum = Constant<T>.Zero;
                for (int j = 0; j < columns; j++)
                {
                    sum = Compute.Add(sum, Compute.Multiply(A[i_times_columns + j], B[j]));
                }
                C[i] = sum;
            }
        }

        /// <summary>Does a matrix-vector multiplication.</summary>
		/// <param name="a">The left matrix of the multiplication.</param>
		/// <param name="b">The right vector of the multiplication.</param>
		/// <returns>The resulting vector of the multiplication.</returns>
		public static Vector<T> Multiply(Matrix<T> a, Vector<T> b)
        {
            Vector<T> c = null;
            Multiply(a, b, ref c);
            return c;
        }

        /// <summary>Does a matrix-vector multiplication.</summary>
		/// <param name="a">The left matrix of the multiplication.</param>
		/// <param name="b">The right vector of the multiplication.</param>
		/// <returns>The resulting vector of the multiplication.</returns>
        public static Vector<T> operator *(Matrix<T> a, Vector<T> b)
        {
            return Multiply(a, b);
        }

        /// <summary>Does a matrix-vector multiplication.</summary>
        /// <param name="b">The right vector of the multiplication.</param>
        /// <param name="c">The resulting vector of the multiplication.</param>
		public void Multiply(Vector<T> b, ref Vector<T> c)
        {
            Multiply(this, b, ref c);
        }

        /// <summary>Does a matrix-vector multiplication.</summary>
		/// <param name="b">The right vector of the multiplication.</param>
		/// <returns>The resulting vector of the multiplication.</returns>
		public Vector<T> Multiply(Vector<T> b)
        {
            return this * b;
        }

        #endregion

        #region Multiply (Matrix * Scalar)

        /// <summary>Multiplies all the values in a matrix by a scalar.</summary>
		/// <param name="a">The matrix to have the values multiplied.</param>
		/// <param name="b">The scalar to multiply the values by.</param>
        /// <param name="c">The resulting matrix after the multiplications.</param>
        private static void Multiply(Matrix<T> a, T b, ref Matrix<T> c)
        {
            if (a is null)
            {
                throw new ArgumentNullException(nameof(a));
            }
            T[] A = a._matrix;
            int Length = A.Length;
            T[] C;
            if (c != null && c._matrix.Length == Length)
            {
                C = c._matrix;
                c._rows = a._rows;
                c._columns = a._columns;
            }
            else
            {
                c = new Matrix<T>(a._rows, a._columns, Length);
                C = c._matrix;
            }
            for (int i = 0; i < Length; i++)
            {
                C[i] = Compute.Multiply(A[i], b);
            }
        }

        /// <summary>Multiplies all the values in a matrix by a scalar.</summary>
		/// <param name="a">The matrix to have the values multiplied.</param>
		/// <param name="b">The scalar to multiply the values by.</param>
		/// <returns>The resulting matrix after the multiplications.</returns>
		public static Matrix<T> Multiply(Matrix<T> a, T b)
        {
            Matrix<T> c = null;
            Multiply(a, b, ref c);
            return c;
        }

        /// <summary>Multiplies all the values in a matrix by a scalar.</summary>
        /// <param name="b">The scalar to multiply the values by.</param>
		/// <param name="a">The matrix to have the values multiplied.</param>
		/// <returns>The resulting matrix after the multiplications.</returns>
		public static Matrix<T> Multiply(T b, Matrix<T> a)
        {
            return Multiply(a, b);
        }

        /// <summary>Multiplies all the values in a matrix by a scalar.</summary>
		/// <param name="a">The matrix to have the values multiplied.</param>
		/// <param name="b">The scalar to multiply the values by.</param>
		/// <returns>The resulting matrix after the multiplications.</returns>
        public static Matrix<T> operator *(Matrix<T> a, T b)
        {
            return Multiply(a, b);
        }

        /// <summary>Multiplies all the values in a matrix by a scalar.</summary>
        /// <param name="b">The scalar to multiply the values by.</param>
        /// <param name="a">The matrix to have the values multiplied.</param>
        /// <returns>The resulting matrix after the multiplications.</returns>
        public static Matrix<T> operator *(T b, Matrix<T> a)
        {
            return Multiply(b, a);
        }

        /// <summary>Multiplies all the values in a matrix by a scalar.</summary>
		/// <param name="b">The scalar to multiply the values by.</param>
        /// <param name="c">The resulting matrix after the multiplications.</param>
		public void Multiply(T b, ref Matrix<T> c)
        {
            Multiply(this, b, ref c);
        }

        /// <summary>Multiplies all the values in a matrix by a scalar.</summary>
		/// <param name="b">The scalar to multiply the values by.</param>
		/// <returns>The resulting matrix after the multiplications.</returns>
		public Matrix<T> Multiply(T b)
        {
            return this * b;
        }

        #endregion

        #region Divide (Matrix / Scalar)

        /// <summary>Divides all the values in the matrix by a scalar.</summary>
        /// <param name="a">The matrix to divide the values of.</param>
        /// <param name="b">The scalar to divide all the matrix values by.</param>
        /// <param name="c">The resulting matrix after the division.</param>
        private static void Divide(Matrix<T> a, T b, ref Matrix<T> c)
        {
            if (a is null)
            {
                throw new ArgumentNullException(nameof(a));
            }
            T[] A = a._matrix;
            int Length = A.Length;
            T[] C;
            if (c != null && c._matrix.Length == Length)
            {
                C = c._matrix;
                c._rows = a._rows;
                c._columns = a._columns;
            }
            else
            {
                c = new Matrix<T>(a._rows, a._columns, Length);
                C = c._matrix;
            }
            for (int i = 0; i < Length; i++)
            {
                C[i] = Compute.Divide(A[i], b);
            }
        }


        /// <summary>Divides all the values in the matrix by a scalar.</summary>
        /// <param name="a">The matrix to divide the values of.</param>
        /// <param name="b">The scalar to divide all the matrix values by.</param>
        /// <returns>The resulting matrix after the division.</returns>
        public static Matrix<T> Divide(Matrix<T> a, T b)
        {
            Matrix<T> c = null;
            Divide(a, b, ref c);
            return c;
        }

        /// <summary>Divides all the values in the matrix by a scalar.</summary>
        /// <param name="a">The matrix to divide the values of.</param>
        /// <param name="b">The scalar to divide all the matrix values by.</param>
        /// <returns>The resulting matrix after the division.</returns>
        public static Matrix<T> operator /(Matrix<T> a, T b)
        {
            return Divide(a, b);
        }

        /// <summary>Divides all the values in the matrix by a scalar.</summary>
        /// <param name="b">The scalar to divide all the matrix values by.</param>
        /// <param name="c">The resulting matrix after the division.</param>
        public void Divide(T b, ref Matrix<T> c)
        {
            Divide(this, b, ref c);
        }

        /// <summary>Divides all the values in the matrix by a scalar.</summary>
        /// <param name="b">The scalar to divide all the matrix values by.</param>
        /// <returns>The resulting matrix after the division.</returns>
		public Matrix<T> Divide(T b)
        {
            return this / b;
        }

        #endregion

        #region Power (Matrix ^ Scalar)

        /// <summary>Applies a power to a square matrix.</summary>
		/// <param name="a">The matrix to be powered by.</param>
		/// <param name="b">The power to apply to the matrix.</param>
        /// <param name="c">The resulting matrix of the power operation.</param>
        private static void Power(Matrix<T> a, int b, ref Matrix<T> c)
        {
            if (a is null)
            {
                throw new ArgumentNullException(nameof(a));
            }
            if (!a.IsSquare)
            {
                throw new MathematicsException("Invalid power (!" + nameof(a) + ".IsSquare)");
            }
            if (b < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(b), b, "!(" + nameof(b) + " >= 0)");
            }
            if (b == 0)
            {
                if (c != null && c._matrix.Length == a._matrix.Length)
                {
                    c._rows = a._rows;
                    c._columns = a._columns;
                    Fill(c, (x, y) => x == y ? Constant<T>.One : Constant<T>.Zero);
                }
                else
                {
                    c = Matrix<T>.FactoryIdentity(a._rows, a._columns);
                }
                return;
            }
            if (c != null && c._matrix.Length == a._matrix.Length)
            {
                c._rows = a._rows;
                c._columns = a._columns;
                T[] A = a._matrix;
                T[] C = c._matrix;
                for (int i = 0; i < a._matrix.Length; i++)
                {
                    C[i] = A[i];
                }
            }
            else
            {
                c = a.Clone();
            }
            Matrix<T> d = new Matrix<T>(a._rows, a._columns, a._matrix.Length);
            for (int i = 0; i < b; i++)
            {
                Multiply(c, c, ref d);
                Matrix<T> temp = d;
                d = c;
                c = d;
            }
        }

        /// <summary>Applies a power to a square matrix.</summary>
		/// <param name="a">The matrix to be powered by.</param>
		/// <param name="b">The power to apply to the matrix.</param>
		/// <returns>The resulting matrix of the power operation.</returns>
		public static Matrix<T> Power(Matrix<T> a, int b)
        {
            Matrix<T> c = null;
            Power(a, b, ref c);
            return c;
        }

        /// <summary>Applies a power to a square matrix.</summary>
		/// <param name="a">The matrix to be powered by.</param>
		/// <param name="b">The power to apply to the matrix.</param>
		/// <returns>The resulting matrix of the power operation.</returns>
        public static Matrix<T> operator ^(Matrix<T> a, int b)
        {
            return Power(a, b);
        }

        /// <summary>Applies a power to a square matrix.</summary>
		/// <param name="b">The power to apply to the matrix.</param>
        /// <param name="c">The resulting matrix of the power operation.</param>
		public void Power(int b, ref Matrix<T> c)
        {
            Power(this, b, ref c);
        }

        /// <summary>Applies a power to a square matrix.</summary>
		/// <param name="b">The power to apply to the matrix.</param>
		/// <returns>The resulting matrix of the power operation.</returns>
		public Matrix<T> Power(int b)
        {
            return this ^ b;
        }

        #endregion

        #region Determinent

        /// <summary>Computes the determinent of a square matrix.</summary>
        /// <param name="a">The matrix to compute the determinent of.</param>
		/// <returns>The computed determinent.</returns>
        public static T Determinent(Matrix<T> a)
        {
            if (a is null)
            {
                throw new ArgumentNullException(nameof(a));
            }
            if (!a.IsSquare)
            {
                throw new MathematicsException("Argument invalid !(" + nameof(a) + "." + nameof(a.IsSquare) + ")");
            }
            T determinant = Constant<T>.One;
            Matrix<T> rref = a.Clone();
            int a_rows = a._rows;
            for (int i = 0; i < a_rows; i++)
            {
                if (Compute.Equal(rref[i, i], Constant<T>.Zero))
                {
                    for (int j = i + 1; j < rref.Rows; j++)
                    {
                        if (Compute.NotEqual(rref.Get(j, i), Constant<T>.Zero))
                        {
                            SwapRows(rref, i, j);
                            determinant = Compute.Multiply(determinant, Constant<T>.NegativeOne);
                        }
                    }
                }
                determinant = Compute.Multiply(determinant, rref.Get(i, i));
                T temp_rowMultiplication = Compute.Divide(Constant<T>.One, rref.Get(i, i));
                RowMultiplication(rref, i, temp_rowMultiplication);
                for (int j = i + 1; j < rref.Rows; j++)
                {
                    T scalar = Compute.Negate(rref.Get(j, i));
                    RowAddition(rref, j, i, scalar);

                }
                for (int j = i - 1; j >= 0; j--)
                {
                    T scalar = Compute.Negate(rref.Get(j, i));
                    RowAddition(rref, j, i, scalar);

                }
            }
            return determinant;
        }

        /// <summary>Computes the determinent of a square matrix.</summary>
		/// <returns>The computed determinent.</returns>
		public T Determinent()
        {
            return Determinent(this);
        }

        #endregion
        
        #region Minor

        /// <summary>Gets the minor of a matrix.</summary>
        /// <param name="a">The matrix to get the minor of.</param>
        /// <param name="row">The restricted row to form the minor.</param>
        /// <param name="column">The restricted column to form the minor.</param>
        /// <param name="b">The minor of the matrix.</param>
        private static void Minor(Matrix<T> a, int row, int column, ref Matrix<T> b)
        {
            if (a is null)
            {
                throw new ArgumentNullException(nameof(a));
            }
            if (a._rows < 2 || a._columns < 2)
            {
                throw new MathematicsException("Argument invalid !(" + nameof(a) + "." + nameof(a.Rows) + " >= 2 && " + nameof(a) + "." + nameof(a.Columns) + " >= 2)");
            }
            if (row < 0 || row >= a._rows)
            {
                throw new ArgumentOutOfRangeException(nameof(row), row, "!(" + nameof(row) + " > 0)");
            }
            if (column < 0 || column >= a._columns)
            {
                throw new ArgumentOutOfRangeException(nameof(column), column, "!(" + nameof(column) + " > 0)");
            }
            if (object.ReferenceEquals(a, b))
            {
                a = a.Clone();
            }
            int a_rows = a._rows;
            int a_columns = a._columns;
            int b_rows = a_rows - 1;
            int b_columns = a_columns - 1;
            int b_length = b_rows * b_columns;
            if (b is null || b._matrix.Length != b_length)
            {
                b = new Matrix<T>(b_rows, b_columns, b_length);
            }
            else
            {
                b._rows = b_rows;
                b._columns = b_columns;
            }
            T[] B = b._matrix;
            T[] A = a._matrix;
            int m = 0, n = 0;
            for (int i = 0; i < a_rows; i++)
            {
                if (i == row)
                {
                    continue;
                }
                int i_times_a_columns = i * a_columns;
                int m_times_b_columns = m * b_columns;
                n = 0;
                for (int j = 0; j < a_columns; j++)
                {
                    if (j == column)
                    {
                        continue;
                    }
                    T temp = A[i_times_a_columns + j];
                    B[m_times_b_columns + n] = temp;
                    n++;
                }
                m++;
            }
        }

        /// <summary>Gets the minor of a matrix.</summary>
		/// <param name="a">The matrix to get the minor of.</param>
		/// <param name="row">The restricted row to form the minor.</param>
		/// <param name="column">The restricted column to form the minor.</param>
		/// <returns>The minor of the matrix.</returns>
		public static Matrix<T> Minor(Matrix<T> a, int row, int column)
        {
            Matrix<T> b = null;
            Minor(a, row, column, ref b);
            return b;
        }

        /// <summary>Gets the minor of a matrix.</summary>
        /// <param name="row">The restricted row to form the minor.</param>
        /// <param name="column">The restricted column to form the minor.</param>
        /// <param name="b">The minor of the matrix.</param>
		public void Minor(int row, int column, ref Matrix<T> b)
        {
            Minor(this, row, column, ref b);
        }

        /// <summary>Gets the minor of a matrix.</summary>
		/// <param name="row">The restricted row to form the minor.</param>
		/// <param name="column">The restricted column to form the minor.</param>
		/// <returns>The minor of the matrix.</returns>
		public Matrix<T> Minor(int row, int column)
        {
            return Minor(this, row, column);
        }

        #endregion

        #region ConcatenateRowWise

        /// <summary>Combines two matrices from left to right 
		/// (result.Rows = left.Rows && result.Columns = left.Columns + right.Columns).</summary>
		/// <param name="a">The left matrix of the concatenation.</param>
		/// <param name="b">The right matrix of the concatenation.</param>
        /// <param name="c">The resulting matrix of the concatenation.</param>
        private static void ConcatenateRowWise(Matrix<T> a, Matrix<T> b, ref Matrix<T> c)
        {
            if (a is null)
            {
                throw new ArgumentNullException(nameof(a));
            }
            if (b is null)
            {
                throw new ArgumentNullException(nameof(a));
            }
            if (a._rows != b._rows)
            {
                throw new MathematicsException("Arguments invalid !(" + nameof(a) + "." + nameof(a.Rows) + " == " + nameof(b) + "." + nameof(b.Rows) + ")");
            }
            int a_columns = a._columns;
            int b_columns = b._columns;
            int c_rows = a._rows;
            int c_columns = a._columns + b._columns;
            int c_length = c_rows * c_columns;
            if (c is null ||
                c._matrix.Length != c_length ||
                object.ReferenceEquals(a, c) ||
                object.ReferenceEquals(b, c))
            {
                c = new Matrix<T>(c_rows, c_columns, c_length);
            }
            else
            {
                c._rows = c_rows;
                c._columns = c_columns;
            }
            T[] A = a._matrix;
            T[] B = b._matrix;
            T[] C = c._matrix;
            for (int i = 0; i < c_rows; i++)
            {
                int i_times_a_columns = i * a_columns;
                int i_times_b_columns = i * b_columns;
                int i_times_c_columns = i * c_columns;
                for (int j = 0; j < c_columns; j++)
                {
                    if (j < a_columns)
                    {
                        C[i_times_c_columns + j] = A[i_times_a_columns + j];
                    }
                    else
                    {
                        C[i_times_c_columns + j] = B[i_times_b_columns + j - a_columns];
                    }
                }
            }
        }

        /// <summary>Combines two matrices from left to right 
		/// (result.Rows = left.Rows && result.Columns = left.Columns + right.Columns).</summary>
		/// <param name="a">The left matrix of the concatenation.</param>
		/// <param name="b">The right matrix of the concatenation.</param>
		/// <returns>The resulting matrix of the concatenation.</returns>
		public static Matrix<T> ConcatenateRowWise(Matrix<T> a, Matrix<T> b)
        {
            Matrix<T> c = null;
            ConcatenateRowWise(a, b, ref c);
            return c;
        }

        /// <summary>Combines two matrices from left to right 
		/// (result.Rows = left.Rows && result.Columns = left.Columns + right.Columns).</summary>
		/// <param name="b">The right matrix of the concatenation.</param>
        /// <param name="c">The resulting matrix of the concatenation.</param>
        public void ConcatenateRowWise(Matrix<T> b, ref Matrix<T> c)
        {
            ConcatenateRowWise(this, b, ref c);
        }

        /// <summary>Combines two matrices from left to right 
		/// (result.Rows = left.Rows && result.Columns = left.Columns + right.Columns).</summary>
		/// <param name="b">The right matrix of the concatenation.</param>
		/// <returns>The resulting matrix of the concatenation.</returns>
        public Matrix<T> ConcatenateRowWise(Matrix<T> b)
        {
            return ConcatenateRowWise(this, b);
        }

        #endregion

        #region Echelon

        /// <summary>Calculates the echelon of a matrix (aka REF).</summary>
        /// <param name="a">The matrix to calculate the echelon of (aka REF).</param>
        /// <param name="b">The echelon of the matrix (aka REF).</param>
        private static void Echelon(Matrix<T> a, ref Matrix<T> b)
        {
            if (a is null)
            {
                throw new ArgumentNullException(nameof(a));
            }
            if (object.ReferenceEquals(a, b))
            {
                a = a.Clone();
            }
            int Rows = a.Rows;
            if (b != null && b._matrix.Length == a._matrix.Length)
            {
                b._rows = Rows;
                b._columns = a._columns;
                CloneContents(a, b);
            }
            else
            {
                b = a.Clone();
            }
            for (int i = 0; i < Rows; i++)
            {
                if (Compute.Equal(b.Get(i, i), Constant<T>.Zero))
                {
                    for (int j = i + 1; j < Rows; j++)
                    {
                        if (Compute.NotEqual(b.Get(j, i), Constant<T>.Zero))
                        {
                            SwapRows(b, i, j);
                        }
                    }
                }
                if (Compute.Equal(b.Get(i, i), Constant<T>.Zero))
                {
                    continue;
                }
                if (Compute.NotEqual(b.Get(i, i), Constant<T>.One))
                {
                    for (int j = i + 1; j < Rows; j++)
                    {
                        if (Compute.Equal(b.Get(j, i), Constant<T>.One))
                        {
                            SwapRows(b, i, j);
                        }
                    }
                }
                T rowMultipier = Compute.Divide(Constant<T>.One, b.Get(i, i));
                RowMultiplication(b, i, rowMultipier);
                for (int j = i + 1; j < Rows; j++)
                {
                    T rowAddend = Compute.Negate(b.Get(j, i));
                    RowAddition(b, j, i, rowAddend);
                }
            }
        }

        /// <summary>Calculates the echelon of a matrix (aka REF).</summary>
		/// <param name="a">The matrix to calculate the echelon of (aka REF).</param>
		/// <returns>The echelon of the matrix (aka REF).</returns>
		public static Matrix<T> Echelon(Matrix<T> a)
        {
            Matrix<T> b = null;
            Echelon(a, ref b);
            return b;
        }

        /// <summary>Calculates the echelon of a matrix (aka REF).</summary>
        /// <param name="b">The echelon of the matrix (aka REF).</param>
		public void Echelon(ref Matrix<T> b)
        {
            Echelon(this, ref b);
        }

        /// <summary>Calculates the echelon of a matrix (aka REF).</summary>
		/// <returns>The echelon of the matrix (aka REF).</returns>
		public Matrix<T> Echelon()
        {
            return Echelon(this);
        }

        #endregion

        #region ReducedEchelon

        /// <summary>Calculates the echelon of a matrix and reduces it (aka RREF).</summary>
		/// <param name="a">The matrix matrix to calculate the reduced echelon of (aka RREF).</param>
        /// <param name="b">The reduced echelon of the matrix (aka RREF).</param>
        private static void ReducedEchelon(Matrix<T> a, ref Matrix<T> b)
        {
            if (a is null)
            {
                throw new ArgumentNullException(nameof(a));
            }
            if (object.ReferenceEquals(a, b))
            {
                a = a.Clone();
            }
            int Rows = a.Rows;
            if (b != null && b._matrix.Length == a._matrix.Length)
            {
                b._rows = Rows;
                b._columns = a._columns;
                CloneContents(a, b);
            }
            else
            {
                b = a.Clone();
            }
            for (int i = 0; i < Rows; i++)
            {
                if (Compute.Equal(b.Get(i, i), Constant<T>.Zero))
                {
                    for (int j = i + 1; j < Rows; j++)
                    {
                        if (Compute.NotEqual(b.Get(j, i), Constant<T>.Zero))
                        {
                            SwapRows(b, i, j);
                        }
                    }
                }
                if (Compute.Equal(b.Get(i, i), Constant<T>.Zero))
                {
                    continue;
                }
                if (Compute.NotEqual(b.Get(i, i), Constant<T>.One))
                {
                    for (int j = i + 1; j < Rows; j++)
                    {
                        if (Compute.Equal(b.Get(j, i), Constant<T>.One))
                        {
                            SwapRows(b, i, j);
                        }
                    }
                }
                T rowMiltiplier = Compute.Divide(Constant<T>.One, b.Get(i, i));
                RowMultiplication(b, i, rowMiltiplier);
                for (int j = i + 1; j < Rows; j++)
                {
                    T rowAddend = Compute.Negate(b.Get(j, i));
                    RowAddition(b, j, i, rowAddend);
                }
                for (int j = i - 1; j >= 0; j--)
                {
                    T rowAddend = Compute.Negate(b.Get(j, i));
                    RowAddition(b, j, i, rowAddend);
                }
            }
        }

        /// <summary>Calculates the echelon of a matrix and reduces it (aka RREF).</summary>
		/// <param name="a">The matrix matrix to calculate the reduced echelon of (aka RREF).</param>
		/// <returns>The reduced echelon of the matrix (aka RREF).</returns>
		public static Matrix<T> ReducedEchelon(Matrix<T> a)
        {
            Matrix<T> b = null;
            ReducedEchelon(a, ref b);
            return b;
        }

        /// <summary>Calculates the echelon of a matrix and reduces it (aka RREF).</summary>
        /// <param name="b">The reduced echelon of the matrix (aka RREF).</param>
        public void ReducedEchelon(ref Matrix<T> b)
        {
            ReducedEchelon(this, ref b);
        }

        /// <summary>Matrixs the reduced echelon form of this matrix (aka RREF).</summary>
		/// <returns>The computed reduced echelon form of this matrix (aka RREF).</returns>
		public Matrix<T> ReducedEchelon()
        {
            return ReducedEchelon(this);
        }

        #endregion

        #region Inverse

        /// <summary>Calculates the inverse of a matrix.</summary>
		/// <param name="a">The matrix to calculate the inverse of.</param>
        /// <param name="b">The inverse of the matrix.</param>
        private static void Inverse(Matrix<T> a, ref Matrix<T> b)
        {
            throw new NotImplementedException();

            //			Matrix<T>.Matrix_Inverse =
            //				Meta.Compile<Matrix<T>.Delegates.Matrix_Inverse>(
            //					string.Concat(
            //@"(Matrix<", T_Source, @">.Delegates.Matrix_Inverse)(
            //(Matrix<", T_Source, @"> _matrix) =>
            //{
            //	if (object.ReferenceEquals(_matrix, null))
            //		throw new System.ArgumentNullException(", "\"matrix\"", @");
            //	if (Matrix<", T_Source, @">.Determinent(_matrix) == 0)
            //		throw new System.ArithmeticException(", "\"inverse calculation failed.\"", @");
            //	Matrix<", T_Source, @"> identity = Matrix<", T_Source, @">.FactoryIdentity(_matrix.Rows, _matrix.Columns);
            //	Matrix<", T_Source, @"> rref = _matrix.Clone();
            //	for (int i = 0; i < _matrix.Rows; i++)
            //	{
            //		if (rref[i, i] == 0)
            //			for (int j = i + 1; j < rref.Rows; j++)
            //				if (rref[j, i] != 0)
            //				{
            //					", SwapRows("temp", "k", "rref", "i", "j"), @"
            //					", SwapRows("temp", "k", "identity", "i", "j"), @"
            //				}
            //		", T_Source, @" temp_rowMultiplication1 = 1 / rref[i, i];
            //		", RowMultiplication("j", "identity", "i", "temp_rowMultiplication1"), @"
            //		", T_Source, @" temp_rowMultiplication2 = 1 / rref[i, i];
            //		", RowMultiplication("j", "rref", "i", "temp_rowMultiplication2"), @"
            //		for (int j = i + 1; j < rref.Rows; j++)
            //		{
            //			", T_Source, @" scalar1 = -rref[j, i];
            //			", RowAddition("k", "identity", "j", "i", "scalar1"), @"
            //			", T_Source, @" scalar2 = -rref[j, i];
            //			", RowAddition("k", "rref", "j", "i", "scalar2"), @"
            //		}
            //		for (int j = i - 1; j >= 0; j--)
            //		{
            //			", T_Source, @" scalar1 = -rref[j, i];
            //			", RowAddition("k", "identity", "j", "i", "scalar1"), @"
            //			", T_Source, @" scalar2 = -rref[j, i];
            //			", RowAddition("k", "rref", "j", "i", "scalar2"), @"
            //		}
            //	}
            //	return identity;
            //})"));

            //			return Matrix<T>.Matrix_Inverse(matrix);

            #region Alternate Version
            //Matrix<T> identity = Matrix<T>.FactoryIdentity(matrix.Rows, matrix.Columns);
            //Matrix<T> rref = matrix.Clone();
            //for (int i = 0; i < matrix.Rows; i++)
            //{
            //	if (Compute.Equate(rref[i, i], Compute.FromInt32(0)))
            //		for (int j = i + 1; j < rref.Rows; j++)
            //			if (!Compute.Equate(rref[j, i], Compute.FromInt32(0)))
            //			{
            //				Matrix<T>.SwapRows(rref, i, j);
            //				Matrix<T>.SwapRows(identity, i, j);
            //			}
            //	Matrix<T>.RowMultiplication(identity, i, Compute.Divide(Compute.FromInt32(1), rref[i, i]));
            //	Matrix<T>.RowMultiplication(rref, i, Compute.Divide(Compute.FromInt32(1), rref[i, i]));
            //	for (int j = i + 1; j < rref.Rows; j++)
            //	{
            //		Matrix<T>.RowAddition(identity, j, i, Compute.Negate(rref[j, i]));
            //		Matrix<T>.RowAddition(rref, j, i, Compute.Negate(rref[j, i]));
            //	}
            //	for (int j = i - 1; j >= 0; j--)
            //	{
            //		Matrix<T>.RowAddition(identity, j, i, Compute.Negate(rref[j, i]));
            //		Matrix<T>.RowAddition(rref, j, i, Compute.Negate(rref[j, i]));
            //	}
            //}
            //return identity;
            #endregion
        }

        /// <summary>Calculates the inverse of a matrix.</summary>
		/// <param name="a">The matrix to calculate the inverse of.</param>
		/// <returns>The inverse of the matrix.</returns>
		public static Matrix<T> Inverse(Matrix<T> a)
        {
            Matrix<T> b = null;
            Inverse(a, ref b);
            return b;
        }

        /// <summary>Matrixs the inverse of this matrix.</summary>
		/// <returns>The inverse of this matrix.</returns>
		public Matrix<T> Inverse()
        {
            return Inverse(this);
        }

        #endregion

        #region Adjoint

        /// <summary>Calculates the adjoint of a matrix.</summary>
		/// <param name="a">The matrix to calculate the adjoint of.</param>
        /// <param name="b">The adjoint of the matrix.</param>
        private static void Adjoint(Matrix<T> a, ref Matrix<T> b)
        {
            if (a is null)
            {
                throw new ArgumentNullException(nameof(a));
            }
            if (!a.IsSquare)
            {
                throw new MathematicsException("Argument invalid !(" + nameof(a) + "." + nameof(a.IsSquare) + ")");
            }
            if (object.ReferenceEquals(a, b))
            {
                a = a.Clone();
            }
            int Length = a.Length;
            int Rows = a.Rows;
            int Columns = a.Columns;
            if (b != null && b.Length == Length)
            {
                b._rows = Rows;
                b._columns = Columns;
            }
            else
            {
                b = new Matrix<T>(Rows, Columns, Length);
            }
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (Compute.IsEven(a.Get(i, j)))
                    {
                        b[i, j] = Determinent(Minor(a, i, j));
                    }
                    else
                    {
                        b[i, j] = Compute.Negate(Determinent(Minor(a, i, j)));
                    }
                }
            }
        }

        /// <summary>Calculates the adjoint of a matrix.</summary>
		/// <param name="a">The matrix to calculate the adjoint of.</param>
		/// <returns>The adjoint of the matrix.</returns>
		public static Matrix<T> Adjoint(Matrix<T> a)
        {
            Matrix<T> b = null;
            Adjoint(a, ref b);
            return b;
        }

        /// <summary>Calculates the adjoint of a matrix.</summary>
        /// <param name="b">The adjoint of the matrix.</param>
		public void Adjoint(ref Matrix<T> b)
        {
            Adjoint(this, ref b);
        }

        /// <summary>Calculates the adjoint of a matrix.</summary>
		/// <returns>The adjoint of the matrix.</returns>
		public Matrix<T> Adjoint()
        {
            return Adjoint(this);
        }

        #endregion

        #region Transpose

        /// <summary>Returns the transpose of a matrix.</summary>
		/// <param name="a">The matrix to transpose.</param>
        /// <param name="b">The transpose of the matrix.</param>
        private static void Transpose(Matrix<T> a, ref Matrix<T> b)
        {
            if (a is null)
            {
                throw new ArgumentNullException(nameof(a));
            }
            if (object.ReferenceEquals(a, b))
            {
                TransposeContents(b);
                return;
            }
            int Length = a.Length;
            int Rows = a.Rows;
            int Columns = a.Columns;
            if (b != null && b.Length == a.Length)
            {
                b._rows = Rows;
                b._columns = Columns;
            }
            else
            {
                b = new Matrix<T>(Rows, Columns, Length);
            }
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    b.Set(i, j, a.Get(j, i));
                }
            }
        }

        /// <summary>Returns the transpose of a matrix.</summary>
		/// <param name="a">The matrix to transpose.</param>
		/// <returns>The transpose of the matrix.</returns>
		public static Matrix<T> Transpose(Matrix<T> a)
        {
            Matrix<T> b = null;
            Transpose(a, ref b);
            return b;
        }

        /// <summary>Returns the transpose of a matrix.</summary>
        /// <param name="b">The transpose of the matrix.</param>
		public void Transpose(ref Matrix<T> b)
        {
            Transpose(this, ref b);
        }

        /// <summary>Returns the transpose of a matrix.</summary>
		/// <returns>The transpose of the matrix.</returns>
		public Matrix<T> Transpose()
        {
            return Transpose(this);
        }

        #endregion

        #region DecomposeLowerUpper

        /// <summary>Decomposes a matrix into lower-upper reptresentation.</summary>
		/// <param name="matrix">The matrix to decompose.</param>
		/// <param name="lower">The computed lower triangular matrix.</param>
		/// <param name="upper">The computed upper triangular matrix.</param>
        private static void DecomposeLowerUpper(Matrix<T> matrix, ref Matrix<T> lower, ref Matrix<T> upper)
        {
            throw new NotImplementedException();

            //			Matrix<T>.Matrix_DecomposeLU =
            //				Meta.Compile<Matrix<T>.Delegates.Matrix_DecomposeLU>(
            //					string.Concat(
            //"(Matrix<", T_Source, "> _matrix, out Matrix<", T_Source, "> _Lower, out Matrix<", T_Source, @"> _Upper) =>
            //{
            //	if (object.ReferenceEquals(_matrix, null))
            //		throw new System.Exception(", "\"null reference: _matrix\"", @");
            //	if (_matrix.Rows != _matrix.Columns)
            //		throw new System.Exception(", "\"non-square _matrix during DecomposeLU function\"", @");
            //	_Lower = Matrix<", T_Source, @">.FactoryIdentity(_matrix.Rows, _matrix.Columns);
            //	_Upper = _matrix.Clone();
            //	int[] permutation = new int[_matrix.Rows];
            //	for (int i = 0; i < _matrix.Rows; i++) permutation[i] = i;
            //	", T_Source, @" p = 0, pom2, detOfP = 1;
            //	int k0 = 0, pom1 = 0;
            //	for (int k = 0; k < _matrix.Columns - 1; k++)
            //	{
            //		p = 0;
            //		for (int i = k; i < _matrix.Rows; i++)
            //				if ((_Upper[i, k] > 0 ? _Upper[i, k] : -_Upper[i, k]) > p)
            //				{
            //						p = _Upper[i, k] > 0 ? _Upper[i, k] : -_Upper[i, k];
            //						k0 = i;
            //				}
            //		if (p == 0)
            //				throw new System.Exception(", "\"The _matrix is singular!\"", @");
            //		pom1 = permutation[k];
            //		permutation[k] = permutation[k0];
            //		permutation[k0] = pom1;
            //		for (int i = 0; i < k; i++)
            //		{
            //				pom2 = _Lower[k, i];
            //				_Lower[k, i] = _Lower[k0, i];
            //				_Lower[k0, i] = pom2;
            //		}
            //		if (k != k0)
            //				detOfP *= -1;
            //		for (int i = 0; i < _matrix.Columns; i++)
            //		{
            //				pom2 = _Upper[k, i];
            //				_Upper[k, i] = _Upper[k0, i];
            //				_Upper[k0, i] = pom2;
            //		}
            //		for (int i = k + 1; i < _matrix.Rows; i++)
            //		{
            //				_Lower[i, k] = _Upper[i, k] / _Upper[k, k];
            //				for (int j = k; j < _matrix.Columns; j++)
            //					_Upper[i, j] = _Upper[i, j] - _Lower[i, k] * _Upper[k, j];
            //		}
            //	}
            //}"));

            //			Matrix<T>.Matrix_DecomposeLU(matrix, out lower, out upper);

            #region Alternate Version
            //lower = Matrix<T>.FactoryIdentity(matrix.Rows, matrix.Columns);
            //upper = matrix.Clone();
            //int[] permutation = new int[matrix.Rows];
            //for (int i = 0; i < matrix.Rows; i++) permutation[i] = i;
            //T p = 0, pom2, detOfP = 1;
            //int k0 = 0, pom1 = 0;
            //for (int k = 0; k < matrix.Columns - 1; k++)
            //{
            //	p = 0;
            //	for (int i = k; i < matrix.Rows; i++)
            //		if ((upper[i, k] > 0 ? upper[i, k] : -upper[i, k]) > p)
            //		{
            //			p = upper[i, k] > 0 ? upper[i, k] : -upper[i, k];
            //			k0 = i;
            //		}
            //	if (p == 0)
            //		throw new System.Exception("The matrix is singular!");
            //	pom1 = permutation[k];
            //	permutation[k] = permutation[k0];
            //	permutation[k0] = pom1;
            //	for (int i = 0; i < k; i++)
            //	{
            //		pom2 = lower[k, i];
            //		lower[k, i] = lower[k0, i];
            //		lower[k0, i] = pom2;
            //	}
            //	if (k != k0)
            //		detOfP *= -1;
            //	for (int i = 0; i < matrix.Columns; i++)
            //	{
            //		pom2 = upper[k, i];
            //		upper[k, i] = upper[k0, i];
            //		upper[k0, i] = pom2;
            //	}
            //	for (int i = k + 1; i < matrix.Rows; i++)
            //	{
            //		lower[i, k] = upper[i, k] / upper[k, k];
            //		for (int j = k; j < matrix.Columns; j++)
            //			upper[i, j] = upper[i, j] - lower[i, k] * upper[k, j];
            //	}
            //}
            #endregion
        }

        /// <summary>Decomposes a matrix into lower-upper reptresentation.</summary>
		/// <param name="lower">The computed lower triangular matrix.</param>
		/// <param name="upper">The computed upper triangular matrix.</param>
		public void DecomposeLowerUpper(ref Matrix<T> lower, ref Matrix<T> upper)
        {
            DecomposeLowerUpper(this, ref lower, ref upper);
        }

        #endregion

        #region Rotate

        /// <summary>Rotates a 4x4 matrix around an 3D axis by a specified angle.</summary>
        /// /// <param name="matrix">The 4x4 matrix to rotate.</param>
        /// <param name="angle">The angle of rotation around the axis.</param>
        /// <param name="axis">The 3D axis to rotate the matrix around.</param>
        /// <returns>The rotated matrix.</returns>
        public static Matrix<T> Rotate4x4(Matrix<T> matrix, Angle<T> angle, Vector<T> axis)
        {
            if (axis is null)
            {
                throw new ArgumentNullException(nameof(axis));
            }
            if (matrix is null)
            {
                throw new ArgumentNullException(nameof(matrix));
            }
            if (axis._vector.Length != 3)
            {
                throw new MathematicsException("Argument invalid !(" + nameof(axis) + "." + nameof(axis.Dimensions) + " == 3)");
            }
            if (matrix._rows != 4 || matrix._columns != 4)
            {
                throw new MathematicsException("Argument invalid !(" + nameof(matrix) + "." + nameof(matrix.Rows) + " == 4 && " + nameof(matrix) + "." + nameof(matrix.Columns) + " == 4)");
            }

            // if the angle is zero, no rotation is required
            if (Compute.Equal(angle._measurement, Constant<T>.Zero))
            {
                return matrix.Clone();
            }

            T cosine = Compute.CosineSystem(angle);
            T sine = Compute.SineSystem(angle);
            T oneMinusCosine = Compute.Subtract(Constant<T>.One, cosine);
            T xy = Compute.Multiply(axis.X, axis.Y);
            T yz = Compute.Multiply(axis.Y, axis.Z);
            T xz = Compute.Multiply(axis.X, axis.Z);
            T xs = Compute.Multiply(axis.X, sine);
            T ys = Compute.Multiply(axis.Y, sine);
            T zs = Compute.Multiply(axis.Z, sine);

            T f00 = Compute.Add(Compute.Multiply(Compute.Multiply(axis.X, axis.X), oneMinusCosine), cosine);
            T f01 = Compute.Add(Compute.Multiply(xy, oneMinusCosine), zs);
            T f02 = Compute.Subtract(Compute.Multiply(xz, oneMinusCosine), ys);
            // n[3] not used
            T f10 = Compute.Subtract(Compute.Multiply(xy, oneMinusCosine), zs);
            T f11 = Compute.Add(Compute.Multiply(Compute.Multiply(axis.Y, axis.Y), oneMinusCosine), cosine);
            T f12 = Compute.Add(Compute.Multiply(yz, oneMinusCosine), xs);
            // n[7] not used
            T f20 = Compute.Add(Compute.Multiply(xz, oneMinusCosine), ys);
            T f21 = Compute.Subtract(Compute.Multiply(yz, oneMinusCosine), xs);
            T f22 = Compute.Add(Compute.Multiply(Compute.Multiply(axis.Z, axis.Z), oneMinusCosine), cosine);

            // Row 1
            T _0_0 = Compute.Add(Compute.Add(Compute.Multiply(matrix[0, 0], f00), Compute.Multiply(matrix[1, 0], f01)), Compute.Multiply(matrix[2, 0], f02));
            T _0_1 = Compute.Add(Compute.Add(Compute.Multiply(matrix[0, 1], f00), Compute.Multiply(matrix[1, 1], f01)), Compute.Multiply(matrix[2, 1], f02));
            T _0_2 = Compute.Add(Compute.Add(Compute.Multiply(matrix[0, 2], f00), Compute.Multiply(matrix[1, 2], f01)), Compute.Multiply(matrix[2, 2], f02));
            T _0_3 = Compute.Add(Compute.Add(Compute.Multiply(matrix[0, 3], f00), Compute.Multiply(matrix[1, 3], f01)), Compute.Multiply(matrix[2, 3], f02));
            // Row 2
            T _1_0 = Compute.Add(Compute.Add(Compute.Multiply(matrix[0, 0], f10), Compute.Multiply(matrix[1, 0], f11)), Compute.Multiply(matrix[2, 0], f12));
            T _1_1 = Compute.Add(Compute.Add(Compute.Multiply(matrix[0, 1], f10), Compute.Multiply(matrix[1, 1], f11)), Compute.Multiply(matrix[2, 1], f12));
            T _1_2 = Compute.Add(Compute.Add(Compute.Multiply(matrix[0, 2], f10), Compute.Multiply(matrix[1, 2], f11)), Compute.Multiply(matrix[2, 2], f12));
            T _1_3 = Compute.Add(Compute.Add(Compute.Multiply(matrix[0, 3], f10), Compute.Multiply(matrix[1, 3], f11)), Compute.Multiply(matrix[2, 3], f12));
            // Row 3
            T _2_0 = Compute.Add(Compute.Add(Compute.Multiply(matrix[0, 0], f20), Compute.Multiply(matrix[1, 0], f21)), Compute.Multiply(matrix[2, 0], f22));
            T _2_1 = Compute.Add(Compute.Add(Compute.Multiply(matrix[0, 1], f20), Compute.Multiply(matrix[1, 1], f21)), Compute.Multiply(matrix[2, 1], f22));
            T _2_2 = Compute.Add(Compute.Add(Compute.Multiply(matrix[0, 2], f20), Compute.Multiply(matrix[1, 2], f21)), Compute.Multiply(matrix[2, 2], f22));
            T _2_3 = Compute.Add(Compute.Add(Compute.Multiply(matrix[0, 3], f20), Compute.Multiply(matrix[1, 3], f21)), Compute.Multiply(matrix[2, 3], f22));
            // Row 4
            T _3_0 = Constant<T>.Zero;
            T _3_1 = Constant<T>.Zero;
            T _3_2 = Constant<T>.Zero;
            T _3_3 = Constant<T>.One;

            return new Matrix<T>(4, 4, (row, column) =>
            {
                switch (row)
                {
                    case 0:
                        switch (column)
                        {
                            case 0: return _0_0;
                            case 1: return _0_1;
                            case 2: return _0_2;
                            case 3: return _0_3;
                            default: throw new MathematicsException("BUG");
                        }
                    case 1:
                        switch (column)
                        {
                            case 0: return _1_0;
                            case 1: return _1_1;
                            case 2: return _1_2;
                            case 3: return _1_3;
                            default: throw new MathematicsException("BUG");
                        }
                    case 2:
                        switch (column)
                        {
                            case 0: return _2_0;
                            case 1: return _2_1;
                            case 2: return _2_2;
                            case 3: return _2_3;
                            default: throw new MathematicsException("BUG");
                        }
                    case 3:
                        switch (column)
                        {
                            case 0: return _3_0;
                            case 1: return _3_1;
                            case 2: return _3_2;
                            case 3: return _3_3;
                            default: throw new MathematicsException("BUG");
                        }
                    default:
                        throw new MathematicsException("BUG");
                }
            });
        }

        public Matrix<T> Rotate4x4(Angle<T> angle, Vector<T> axis)
        {
            return Rotate4x4(this, angle, axis);
        }

        #endregion

        #region Equal

        /// <summary>Does a value equality check.</summary>
		/// <param name="a">The first matrix to check for equality.</param>
		/// <param name="b">The second matrix to check for equality.</param>
		/// <returns>True if values are equal, false if not.</returns>
        private static bool Equal(Matrix<T> a, Matrix<T> b)
        {
            if (a is null && b is null)
            {
                return true;
            }
            if (a is null)
            {
                return false;
            }
            if (b is null)
            {
                return false;
            }
            int Rows = a.Rows;
            int Columns = a.Columns;
            if (Rows != b.Rows || Columns != b.Columns)
            {
                return false;
            }
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (Compute.NotEqual(a.Get(i, j), b.Get(i, j)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>Does a value equality check.</summary>
		/// <param name="a">The first matrix to check for equality.</param>
		/// <param name="b">The second matrix to check for equality.</param>
		/// <returns>True if values are equal, false if not.</returns>
        public static bool operator ==(Matrix<T> a, Matrix<T> b)
        {
            return Equal(a, b);
        }

        /// <summary>Does a value non-equality check.</summary>
		/// <param name="a">The first matrix to check for non-equality.</param>
		/// <param name="b">The second matrix to check for non-equality.</param>
		/// <returns>True if values are not equal, false if not.</returns>
		public static bool operator !=(Matrix<T> a, Matrix<T> b)
        {
            return !Equal(a, b);
        }

        /// <summary>Does a value equality check.</summary>
		/// <param name="b">The second matrix to check for equality.</param>
		/// <returns>True if values are equal, false if not.</returns>
		public bool Equal(Matrix<T> b)
        {
            return this == b;
        }

        #endregion

        #region Equal (+leniency)

        /// <summary>Does a value equality check with leniency.</summary>
		/// <param name="a">The first matrix to check for equality.</param>
		/// <param name="b">The second matrix to check for equality.</param>
		/// <param name="leniency">How much the values can vary but still be considered equal.</param>
		/// <returns>True if values are equal, false if not.</returns>
        private static bool Equal(Matrix<T> a, Matrix<T> b, T leniency)
        {
            if (a is null && b is null)
            {
                return true;
            }
            if (a is null)
            {
                return false;
            }
            if (b is null)
            {
                return false;
            }
            int Rows = a.Rows;
            int Columns = a.Columns;
            if (Rows != b.Rows || Columns != b.Columns)
            {
                return false;
            }
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (Compute.EqualLeniency(a.Get(i, j), b.Get(i, j), leniency))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>Does a value equality check with leniency.</summary>
		/// <param name="b">The second matrix to check for equality.</param>
		/// <param name="leniency">How much the values can vary but still be considered equal.</param>
		/// <returns>True if values are equal, false if not.</returns>
		public bool Equal(Matrix<T> b, T leniency)
        {
            return Equal(this, b, leniency);
        }

        #endregion

        #endregion

        #region Other Methods

        #region Get/Set

        internal T Get(int row, int column)
        {
            return this._matrix[row * this.Columns + column];
        }

        internal void Set(int row, int column, T value)
        {
            this._matrix[row * this.Columns + column] = value;
        }

        #endregion

        #region Fill

        public static void Fill(Matrix<T> matrix, Func<int, int, T> function)
        {
            int Rows = matrix.Rows;
            int Columns = matrix.Columns;
            T[] MATRIX = matrix._matrix;
            int i = 0;
            for (int row = 0; row < Rows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    MATRIX[i++] = function(row, column);
                }
            }
        }

        #endregion

        #region CloneContents

        internal static void CloneContents(Matrix<T> a, Matrix<T> b)
        {
            T[] a_flat = a._matrix;
            T[] b_flat = b._matrix;
            int length = a_flat.Length;
            for (int i = 0; i < length; i++)
            {
                b_flat[i] = a_flat[i];
            }
        }

        #endregion

        #region TransposeContents

        internal static void TransposeContents(Matrix<T> a)
        {
            int Rows = a.Rows;
            int Columns = a.Columns;
            for (int i = 0; i < Rows; i++)
            {
                int Rows_Minus_i = Rows - i;
                for (int j = 0; j < Rows_Minus_i; j++)
                {
                    T temp = a.Get(i, j);
                    a.Set(i, j, a.Get(j, i));
                    a.Set(j, i, temp);
                }
            }
        }

        #endregion

        #region Clone

        /// <summary>Creates a copy of a matrix.</summary>
        /// <param name="a">The matrix to copy.</param>
        /// <returns>The copy of this matrix.</returns>
        public static Matrix<T> Clone(Matrix<T> a)
        {
            if (a is null)
            {
                throw new ArgumentNullException(nameof(a));
            }
            return new Matrix<T>(a);
        }

        /// <summary>Copies this matrix.</summary>
        /// <returns>The copy of this matrix.</returns>
        public Matrix<T> Clone()
        {
            return Clone(this);
        }

        #endregion

        #endregion

        #region Casting Operators

        /// <summary>Converts a T[,] into a matrix.</summary>
        /// <param name="a">The T[,] to convert to a matrix.</param>
        /// <returns>The resulting matrix after conversion.</returns>
        public static explicit operator Matrix<T>(T[,] array)
        {
            return new Matrix<T>(array.GetLength(0), array.GetLength(1), (i, j) => array[i, j]);
        }

        /// <summary>Converts a matrix into a T[,].</summary>
        /// <param name="a">The matrix toconvert to a T[,].</param>
        /// <returns>The resulting T[,] after conversion.</returns>
        public static explicit operator T[,](Matrix<T> matrix)
        {
            int rows = matrix._rows;
            int columns = matrix._columns;
            T[,] array = new T[rows, columns];
            T[] MATRIX = matrix._matrix;
            int k = 0;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    array[i, j] = MATRIX[k++];
                }
            }
            return array;
        }

        #endregion

        #region Steppers

        /// <summary>Invokes a delegate for each entry in the data structure.</summary>
        /// <param name="step">The delegate to invoke on each item in the structure.</param>
        public void Stepper(Step<T> step)
		{
            for (int i = 0; i < this._matrix.Length; i++)
            {
                step(this._matrix[i]);
            }
		}

        /// <summary>Invokes a delegate for each entry in the data structure.</summary>
        /// <param name="step">The delegate to invoke on each item in the structure.</param>
        public void Stepper(StepRef<T> step)
		{
            for (int i = 0; i < this._matrix.Length; i++)
            {
                step(ref this._matrix[i]);
            }
		}

        /// <summary>Invokes a delegate for each entry in the data structure.</summary>
        /// <param name="step">The delegate to invoke on each item in the structure.</param>
        /// <returns>The resulting status of the iteration.</returns>
        public StepStatus Stepper(StepBreak<T> step)
		{
            for (int i = 0; i < this._matrix.Length; i++)
            {
                if (step(this._matrix[i]) == StepStatus.Break)
                {
                    return StepStatus.Break;
                }
            }
			return StepStatus.Continue;
		}

        /// <summary>Invokes a delegate for each entry in the data structure.</summary>
        /// <param name="step">The delegate to invoke on each item in the structure.</param>
        /// <returns>The resulting status of the iteration.</returns>
        public StepStatus Stepper(StepRefBreak<T> step)
		{
            for (int i = 0; i < this._matrix.Length; i++)
            {
                if (step(ref this._matrix[i]) == StepStatus.Break)
                {
                    return StepStatus.Break;
                }
            }
			return StepStatus.Continue;
		}

		#endregion

		#region Overrides

		/// <summary>Prints out a string representation of this matrix.</summary>
		/// <returns>A string representing this matrix.</returns>
		public override string ToString()
		{
            return base.ToString();
        }

		/// <summary>Matrixs a hash code from the values of this matrix.</summary>
		/// <returns>A hash code for the matrix.</returns>
		public override int GetHashCode()
		{
            return this._matrix.GetHashCode() ^ this._rows ^ this._columns;
        }

		/// <summary>Does an equality check by value.</summary>
		/// <param name="b">The object to compare to.</param>
		/// <returns>True if the references are equal, false if not.</returns>
		public override bool Equals(object b)
		{
            if (!(b is Matrix<T>))
            {
                return Equal(this, (Matrix<T>)b);
            }
			return false;
		}

		#endregion
	}
}
