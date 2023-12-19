using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _2048_MS_Graph
{/// Auteur: Marc Schilter
/// 2048 en Windows Forms
/// CPNV - SI-CA1a - 2023
    public partial class Form1 : Form
    {
        private int[,] board = new int[4, 4];
        private static bool[,] fusionTuile = new bool[4, 4];
        private int score = 0;
        private Timer animationTimer;
        private Dictionary<Point, Point> DepTuile; // Pour gérer les animations
        private List<Tuple<Point, Point>> mouvementTuile = new List<Tuple<Point, Point>>();
        public Form1()
        {
            InitializeComponent(); 
            InitializeGame();  // démarre la partie

            // Configuration du timer pour l'animation
            animationTimer = new Timer();
            animationTimer.Interval = 256; 
            animationTimer.Tick += AnimationTick;
            DepTuile = new Dictionary<Point, Point>();
            //utilisé pour le dessin de la grille et des tuiles dans le panel windows form. 
            panelJeu.Paint += new PaintEventHandler(panelJeu_Paint);
        }
        private void InitializeGame()
        {
            // Réinitialiser le score
            score = 0;

            // Initialiser le plateau de jeu avec des zéros
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    board[i, j] = 0;
                }
            }

            // Boucle pour appeler deux fois la fonction RandomTuile()
            for (int i = 0; i < 2; i++)
            {
                RandomTuile();
            }

            // Dessiner la grille initiale          
            panelJeu.Invalidate();
         
        }       
        private void DrawGrid(Graphics g) // dessin de la grille
        {
            int cellSize = 100; // Taille de la cellule, ajustez selon vos besoins
            Font font = new Font("Arial", 20);

            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    // Couleur de fond basée sur la valeur de la tuile
                    Color color = ColorTuile(board[i, j]);
                    Brush brush = new SolidBrush(color);
                    Rectangle rect = new Rectangle(j * cellSize, i * cellSize, cellSize, cellSize);

                    g.FillRectangle(brush, rect); // Remplir le rectangle avec la couleur
                    g.DrawRectangle(Pens.Black, rect); // Dessiner le contour


                    // Dessiner la valeur de la tuile
                    string text = board[i, j] > 0 ? board[i, j].ToString() : "";
                    g.DrawString(text, font, Brushes.Black, rect, new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    });                   
                }
            }
        }
        private Color ColorTuile(int value) //gestions des couleurs en fonction de la valeur de la tuile. 
        {
            switch (value)
            {
                case 2: return Color.LightSalmon;
                case 4: return Color.LightYellow;               
                case 8: return Color.LightBlue;                   
                case 16: return Color.LightSeaGreen;
                case 32: return Color.LightSteelBlue;
                case 64: return Color.Salmon;
                case 128: return Color.Yellow;
                case 256: return Color.Blue;
                case 512: return Color.SeaGreen;
                case 1024: return Color.SteelBlue;
                case 2048: return Color.DarkSalmon;
                case 4096: return Color.DarkOrchid;
                case 8192: return Color.Chartreuse;
                case 16384: return Color.IndianRed; 
                default: return Color.White;
            }
        }
        private void MajDepTuile()
        {
            // Liste pour garder une trace des tuiles qui ont atteint leur destination
            List<Point> DepFini = new List<Point>();
            // Dictionnaire temporaire pour stocker les nouvelles positions
            var nouvelPosi = new Dictionary<Point, Point>();

            foreach (var item in DepTuile)
            {
                Point start = item.Key;
                Point end = item.Value;

                // Ici, vous calculez la position intermédiaire pour l'animation
                // Par exemple, déplacer la tuile un peu plus près de sa destination à chaque tick
                // Cette logique dépend de la vitesse de l'animation que vous souhaitez

                // Exemple très simple de déplacement
                Point current = new Point(
                    DeplaceVers(start.X, end.X, 1), // Déplacez d'un pixel par tick, par exemple
                    DeplaceVers(start.Y, end.Y, 1));

                if (current == end)
                {
                    DepFini.Add(start);
                }
                else
                {
                    nouvelPosi[start] = current;
                }
            }
            // Appliquer les mises à jour après l'itération
            foreach (var position in nouvelPosi)
            {
                DepTuile[position.Key] = position.Value;
            }

            // Supprimer les tuiles qui ont atteint leur destination
            foreach (var Fini in DepFini)
            {
                DepTuile.Remove(Fini);
            }
            // Si toutes les tuiles ont atteint leur destination
            if (DepTuile.Count == 0)
            {
                animationTimer.Stop(); // Arrêter le timer d'animation
            }
        }    
    // Méthode d'assistance pour déplacer un point vers une cible
    private int DeplaceVers(int start, int end, int step)
          {
            // Réduire la valeur de 'step' pour ralentir l'animation
            step = Math.Max(1, step / 20 ); 

            if (start < end)
                return Math.Min(start + step, end);
            else if (start > end)
                return Math.Max(start - step, end);
            else
                return end; // Si start est égal à end, retourner end
        }
        private void AnimationTick(object sender, EventArgs e)
        {
            // Mise à jour de la position des tuiles en mouvement
            MajDepTuile();

            // Demander le redessinage du panel pour refléter les nouvelles positions des tuiles
            panelJeu.Invalidate();
        }
        private bool PartieGagne()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (board[i, j] == 2048)
                    {
                        var result = MessageBox.Show("Félicitations! Vous avez atteint la tuile 2048. Voulez-vous continuer?",
                                              "Partie Gagnée",
                                              MessageBoxButtons.YesNo,
                                              MessageBoxIcon.Question);
                        if (result == DialogResult.No)
                        {
                            // Code pour terminer le jeu 
                            this.Close(); // Pour fermer le formulaire
                            return true;
                        }
                        return false; //l'utilisateur continue à jouer.
                    }
                }
            }
            return false;
        }
        private bool PartieFini()
        {
            // Vérifiez d'abord s'il y a des tuiles vides
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (board[i, j] == 0)
                    {
                        return false;
                    }
                }
            }

            // Vérifiez ensuite s'il y a des mouvements possibles
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if ((i - 1 >= 0 && board[i, j] == board[i - 1, j]) ||
                        (i + 1 < 4 && board[i, j] == board[i + 1, j]) ||
                        (j - 1 >= 0 && board[i, j] == board[i, j - 1]) ||
                        (j + 1 < 4 && board[i, j] == board[i, j + 1]))
                    {
                        return false;
                    }
                }
            }

            // Si la partie est terminée
            MessageBox.Show("Game Over! Votre score: " + score, "Partie Terminée", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
            return true;
        }

        private void StartAnimation()
        {
            DepTuile.Clear();

            foreach (var movement in mouvementTuile)
            {
                var start = movement.Item1;
                var end = movement.Item2;
                DepTuile[start] = end;
            }

            if (DepTuile.Count > 0)
            {
                animationTimer.Start();
            }
        }
        protected override void OnKeyDown(KeyEventArgs e)  // permet l'utilisation des fléches directionnelles. 
        {
            base.OnKeyDown(e);
            bool moved = false;

            if (e.KeyCode == Keys.Up) moved = DeplaceTuile(0, -1);
            else if (e.KeyCode == Keys.Down) moved = DeplaceTuile(0, 1);
            else if (e.KeyCode == Keys.Left) moved = DeplaceTuile(-1, 0);
            else if (e.KeyCode == Keys.Right) moved = DeplaceTuile(1, 0);

            if (moved)
            {
                RandomTuile();
                // Mettre à jour l'affichage après le mouvement
                panelJeu.Invalidate();
            }
        }
        private void RandomTuile()  // génération de tuile aléatoire
        {
            // Tableau pour stocker les indices des cellules vides dans la grille
            List<Tuple<int, int>> caseVide = new List<Tuple<int, int>>();

            // Parcours de la grille pour trouver toutes les cellules vides
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (board[i, j] == 0) // Si la cellule est vide (contient la valeur 0)
                    {
                        caseVide.Add(new Tuple<int, int>(i, j));
                    }
                }
            }

            // Vérifier s'il y a des cellules vides disponibles
            if (caseVide.Count > 0)
            {
                Random random = new Random();
                // Générer un index aléatoire parmi les cellules vides
                int randomIndex = random.Next(caseVide.Count);

                // Récupérer l'indice de la cellule sélectionnée aléatoirement
                var cell = caseVide[randomIndex];
                int row = cell.Item1;
                int col = cell.Item2;

                // Ajouter une nouvelle tuile (2 ou 4) à la cellule sélectionnée
                board[row, col] = (random.Next(2) + 1) * 2; // Soit 2, soit 4
            }
        }
             private  bool DeplaceTuile(int dh, int dv)
            {
                bool moved = false;
                mouvementTuile.Clear(); // Réinitialiser les mouvements de tuiles pour ce tour


            // Réinitialiser le tableau de fusion à chaque mouvement
            for (int i = 0; i < board.GetLength(0); i++)
                {
                    for (int j = 0; j < board.GetLength(1); j++)
                    {
                        fusionTuile[i, j] = false;
                    }
                }

                // Déterminer la direction du mouvement
                int[] order = new int[4] { 0, 1, 2, 3 };
                if (dh == 1 || dv == 1)
                {
                    order = new int[4] { 3, 2, 1, 0 }; // Inverser l'ordre pour les mouvements vers la droite ou le bas
                }

                // Parcourir la grille
                for (int x = 0; x < board.GetLength(0); x++)
                {
                    for (int y = 0; y < board.GetLength(1); y++)
                    {
                        int i = order[x];
                        int j = order[y];

                        if (board[i, j] == 0) continue; // Ignorer les cases vides

                        // Trouver la prochaine position
                        int nextI = i + dv;
                        int nextJ = j + dh;

                        while (nextI >= 0 && nextI < 4 && nextJ >= 0 && nextJ < 4)
                        {
                            // Vérifier si la case est occupée
                            if (board[nextI, nextJ] != 0)
                            {
                                // Vérifier si une fusion est possible
                                if (board[nextI, nextJ] == board[i, j] && !fusionTuile[nextI, nextJ] && !fusionTuile[i, j])
                                {
                                    board[nextI, nextJ] *= 2;
                                    score += board[nextI, nextJ];
                                    board[i, j] = 0;
                                    fusionTuile[nextI, nextJ] = true;
                                    moved = true;
                                    mouvementTuile.Add(new Tuple<Point, Point>(new Point(i, j), new Point(nextI, nextJ)));
                                    break;
                            }
                                else
                                {
                                    // Déplacer la tuile sans fusionner
                                    if (nextI - dv != i || nextJ - dh != j)
                                    {
                                        board[nextI - dv, nextJ - dh] = board[i, j];
                                        if (nextI - dv != i || nextJ - dh != j) board[i, j] = 0;
                                        moved = true;
                                        mouvementTuile.Add(new Tuple<Point, Point>(new Point(i, j), new Point(nextI - dv, nextJ - dh)));
                                }
                                    break;
                                }
                            }

                            nextI += dv;
                            nextJ += dh;
                        }

                        // Gérer le cas où la tuile se déplace vers une case vide en fin de grille
                        if (nextI < 0 || nextI >= 4 || nextJ < 0 || nextJ >= 4)
                        {
                            nextI -= dv;
                            nextJ -= dh;
                            if (nextI != i || nextJ != j)
                            {
                                board[nextI, nextJ] = board[i, j];
                                board[i, j] = 0;
                                moved = true;
                                mouvementTuile.Add(new Tuple<Point, Point>(new Point(i, j), new Point(nextI, nextJ)));
                        }
                        }
                    }
                }
            if (moved)
            {
                panelJeu.Invalidate();  // Met à jour la grille
                MajScore(); // Met à jour l'affichage du score
                StartAnimation(); // Démarre l'animation si des tuiles ont bougé
                PartieFini();
                PartieGagne();
            }
            return moved;
            }
        private void MajScore()
        {
            // Mettre à jour l'affichage du score
            lblScore.Text = $"2048 - Score: {score}";
        }

        private void panelJeu_Paint(object sender, PaintEventArgs e) // intégre la fonction de dession de la grille dans le panel.
        {
            DrawGrid(e.Graphics);
        }
    }
}
