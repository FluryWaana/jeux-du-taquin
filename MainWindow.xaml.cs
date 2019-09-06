using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace JeuTest
{
    public partial class MainWindow : Window
    {
        // Lettre disponible
        private char[] arrayLettersOP = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', ' ' };

        // Statut de la partie
        private bool PartStart = false;

        private DispatcherTimer timer;
        private TimeSpan time;

        public MainWindow()
        {
            InitializeComponent();

            // Authorise le drag & drop dans la zone du tableau de boutons
            arrayLetters.AllowDrop = true;

            /**
             * Récupération de tous les boutons qui se situe dans le tableau
             * des lettres.
             * Inscription de ceux-ci aux évènements pour le drag and drop
             * 
             * Utilisation de PreviewMouseDown au lieu de MouseDown car celui-ci
             * récupère le clique gauche
             */
            foreach ( Button btn in arrayLetters.Children )
            {
                btn.PreviewMouseDown += BtnPreviewMouseDown;
                btn.Drop             += BtnDragDrop;
                btn.Drop             += Result;
                btn.IsEnabled        = false;
            }

            // Délégue l'action du clique sur la fonction BtnStart
            btnStart.Click += BtnStart;
        }

        private void BtnStart(object sender, EventArgs e)
        {
            // Modifie l'action sur le bouton Nouvelle partie qui devient Recommencer
            btnStart.Click -= BtnStart;
            btnStart.Click += BtnRestart;

            // Modification du content
            btnStart.Content = "Recommencer";

            GameStart();
        }

        private void BtnRestart(object sender, EventArgs e)
        {
            if( ! PartStart )
            {
                GameStart();
            }            
        }

        private void GameStart()
        {
            if( ! PartStart )
            {
                // Tri le tableau Random
                ShuffleArray();

                // Départ du compteur
                Counter();

                // Active les boutons
                btnChangeStatus(true);

                int count = 0;
                foreach (Button btn in arrayLetters.Children)
                {
                    if (arrayLettersOP[count].Equals(' '))
                    {
                        btn.Opacity = 0;
                    }
                    else
                    {
                        btn.Opacity = 100;
                    }

                    btn.Content = arrayLettersOP[count];
                    count++;
                }

                PartStart = true;
            }
        }

        private void Counter()
        {
            time = TimeSpan.FromSeconds(30);

            timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                counter.Content = time.ToString(@"mm\:ss");
                if (time == TimeSpan.Zero)
                {
                    timer.Stop();
                }
                time = time.Add(TimeSpan.FromSeconds(-1));
            }, Application.Current.Dispatcher);

            timer.Tick += TimeEvent;
            timer.Start();
        }

        private void TimeEvent(object sender, EventArgs e )
        {
            if( time.ToString(@"mm\:ss").Equals("00:00") )
            {
                MessageBox.Show("Vous avez perdu!");
                btnChangeStatus(false);
                PartStart = false;
            }
        }

        /**
         * Action lorsque un clique est maintenu
         */
        private void BtnPreviewMouseDown( object sender, MouseEventArgs e )
        {
            // Récupération du bouton
            Button btn = (Button)sender;

            // Données envoyées au Drag &D
            DataObject Data = new DataObject();
            Data.SetData( btn );

            // Si le bouton est vide alors on ne permet pas le Drag & Drop
            if ( ! btn.Content.Equals(" ") )
            {
                DragDrop.DoDragDrop( btn, Data, DragDropEffects.Move );
            }
        }

        /**
         * Action lors du drop sur un bouton 
         */
        private void BtnDragDrop( object sender, DragEventArgs e )
        {
            // Récupère le bouton drop & ses coordonnées
            // as Button == (Button)
            Button btn_drop = e.Data.GetData( typeof(Button)) as Button;
            int Btn_drop_y  = Grid.GetRow(btn_drop);
            int Btn_drop_x  = Grid.GetColumn(btn_drop);

            // Récupère le bouton ciblé  & ses coordonnées
            Button btn_target = (Button)sender;
            int Btn_target_y  = Grid.GetRow(btn_target);
            int Btn_target_x  = Grid.GetColumn(btn_target);

            // Variable temporaire pour sauvegarder une lettre;
            string Temp = btn_drop.Content.ToString();

            /**
             * SI la position Y + 1 du bouton drop est égale à la position Y du bouton target
             * ET que la position X du bouton drop est égale à la position du bouton X du bouton target
             * OU
             * SI la position Y - 1 du bouton drop est égale à la position Y du bouton target
             * ET que la position X du bouton drop est égale à la position du bouton X du bouton target
             * OU
             * SI la position X + 1 du bouton drop est égale à la position Y du bouton target
             * ET que la position Y du bouton drop est égale à la position du bouton Y du bouton target
             * OU
             * SI la position X - 1 du bouton drop est égale à la position Y du bouton target
             * ET que la position Y du bouton drop est égale à la position du bouton Y du bouton target
             * ALORS on assigne les nouvelles valeurs aux boutons
             */
            if( 
                Btn_drop_y + 1 == Btn_target_y && Btn_drop_x == Btn_target_x ||
                Btn_drop_y - 1 == Btn_target_y && Btn_drop_x == Btn_target_x ||
                Btn_drop_x + 1 == Btn_target_x && Btn_drop_y == Btn_target_y ||
                Btn_drop_x - 1 == Btn_target_x && Btn_drop_y == Btn_target_y
             )
            {
                if( ! btn_target.Content.Equals( ' ' ) )
                {
                    // Assigne les nouvelles valeurs
                    btn_drop.Content = btn_target.Content;
                    btn_target.Content = Temp;
                }
            }
        }

        private void Result( object sender, DragEventArgs e )
        {
            string str = "";

            foreach (Button btn in arrayLetters.Children)
            {
                str += btn.Content;
            }

            str = string.Join( "", str.Split( default( string[] ), StringSplitOptions.RemoveEmptyEntries ) );

            if( str.Equals( "ABCDEFGH" ) )
            {
                timer.Stop();
                MessageBox.Show( "Vous avez gagné!" );
                btnChangeStatus(false);
                PartStart = false;
            }
        }

        private void ShuffleArray()
        {
            Random rng = new Random();
            for (int i = arrayLettersOP.Count() - 1; i > 0; i--)
            {
                int swapIndex = rng.Next(i + 1);
                if (swapIndex != i)
                {
                    char tmp = arrayLettersOP[swapIndex];
                    arrayLettersOP[swapIndex] = arrayLettersOP[i];
                    arrayLettersOP[i] = tmp;
                }
            }
        }

        private void btnChangeStatus( bool statut )
        {
            if( statut )
            {
                foreach (Button btn in arrayLetters.Children)
                {
                    btn.IsEnabled = true;
                }
            }
            else
            {
                foreach (Button btn in arrayLetters.Children)
                {
                    btn.IsEnabled = false;
                }
            }
        }
    }
}
