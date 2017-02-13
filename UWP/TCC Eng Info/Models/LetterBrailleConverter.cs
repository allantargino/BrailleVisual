namespace TCC_Eng_Info.Models
{
    /// <summary>
    /// Encapsula a conversão de letras em forma de Char para os pontos Braille de uma cela.
    /// </summary>
    public static class LetterBrailleConverter
    {
        private static readonly bool[] A = new bool[] { true, false, false, false, false, false };
        private static readonly bool[] B = new bool[] { true, false, true, false, false, false };
        private static readonly bool[] C = new bool[] { true, true, false, false, false, false };
        private static readonly bool[] D = new bool[] { true, true, false, true, false, false };
        private static readonly bool[] E = new bool[] { true, false, false, true, false, false };
        private static readonly bool[] F = new bool[] { true, true, true, false, false, false };
        private static readonly bool[] G = new bool[] { true, true, true, true, false, false };
        private static readonly bool[] H = new bool[] { true, false, true, true, false, false };
        private static readonly bool[] I = new bool[] { false, true, true, false, false, false };
        private static readonly bool[] J = new bool[] { false, true, true, true, false, false };
        private static readonly bool[] K = new bool[] { true, false, false, false, true, false };
        private static readonly bool[] L = new bool[] { true, false, true, false, true, false };
        private static readonly bool[] M = new bool[] { true, true, false, false, true, false };
        private static readonly bool[] N = new bool[] { true, true, false, true, true, false };
        private static readonly bool[] O = new bool[] { true, false, false, true, true, false };
        private static readonly bool[] P = new bool[] { true, true, true, false, true, false };
        private static readonly bool[] Q = new bool[] { true, true, true, true, true, false };
        private static readonly bool[] R = new bool[] { true, false, true, true, true, false };
        private static readonly bool[] S = new bool[] { false, true, true, false, true, false };
        private static readonly bool[] T = new bool[] { false, true, true, true, true, false };
        private static readonly bool[] U = new bool[] { true, false, false, false, true, true };
        private static readonly bool[] V = new bool[] { true, false, true, false, true, true };
        private static readonly bool[] W = new bool[] { false, true, true, true, false, true };
        private static readonly bool[] X = new bool[] { true, true, false, false, true, true };
        private static readonly bool[] Y = new bool[] { true, true, false, true, true, true };
        private static readonly bool[] Z = new bool[] { true, false, false, true, true, true };

        private static readonly bool[] Dot = new bool[] { false, false, true, true, false, true };
        private static readonly bool[] Comma = new bool[] { false, false, true, false, false, false };
        private static readonly bool[] Semicolon = new bool[] { false, false, true, false, true, false };
        private static readonly bool[] Hyphen = new bool[] { false, false, false, false, true, true };
        private static readonly bool[] Question = new bool[] { false, false, true, false, false, true };
        private static readonly bool[] Exclamation = new bool[] { false, false, true, true, true, false };

        private static readonly bool[] Empty = new bool[] { false, false, false, false, false, false };

        /// <summary>
        /// Converte um char em um array com 6 coordenadas booleanas Braille.
        /// </summary>
        /// <param name="letter">Letra que será convertida em um array lógico Braille.</param>
        /// <returns>Array de booleanos contendo coordenadas Braille.</returns>
        public static bool[] LetterToBraille(char letter)
        {
            switch (letter)
            {
                case 'A':
                    return A;
                case 'B':
                    return B;
                case 'C':
                    return C;
                case 'D':
                    return D;
                case 'E':
                    return E;
                case 'F':
                    return F;
                case 'G':
                    return G;
                case 'H':
                    return H;
                case 'I':
                    return I;
                case 'J':
                    return J;
                case 'K':
                    return K;
                case 'L':
                    return L;
                case 'M':
                    return M;
                case 'N':
                    return N;
                case 'O':
                    return O;
                case 'P':
                    return P;
                case 'Q':
                    return Q;
                case 'R':
                    return R;
                case 'S':
                    return S;
                case 'T':
                    return T;
                case 'U':
                    return U;
                case 'V':
                    return V;
                case 'W':
                    return W;
                case 'X':
                    return X;
                case 'Y':
                    return Y;
                case 'Z':
                    return Z;

                case '.':
                    return Dot;
                case ',':
                    return Comma;
                case ';':
                    return Semicolon;
                case '-':
                    return Hyphen;
                case '?':
                    return Question;
                case '!':
                    return Exclamation;

                default:
                    return Empty;
            }
        }
    }
}