// Importing namespaces and libraries for WPF application development
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace dog
{
    public partial class MainWindow : Window
    {
        // menu variables
        bool isInSettings = false;
        int interval = 200;
        int size = 10;
        int bones = 1;

        // dog variables
        int points = 0;
        Image? dogHead;
        int headRow = 5;
        int headCol = 1;
        List<Point> dog = new List<Point>();
        List<Image> bodyImages = new List<Image>();
        List<int> bodyTypes = new List<int>();

        // dog direction
        string direction = "right";
        DispatcherTimer? gameTimer;

        // bone variables
        List<Image> boneImages = new List<Image>();
        List<Point> bonePositions = new List<Point>();
        int boneRow;
        int boneCol;
        bool bonePositioned = false;

        // random number generator
        Random random = new Random();



        // constructor
        public MainWindow()
        {
            InitializeComponent();
            buildGamefield();
        }






        // ********** event handlers for settings sliders **********
        // speed
        private void Speed_interval(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            interval = 1000 / (int)e.NewValue;
        }

        // size
        private void Size_interval(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            size = (int)e.NewValue;
            size = (int)e.NewValue;
            buildGamefield();
        }

        // bones
        private void bone_interval(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            bones = (int)e.NewValue;
        }



        // method to show settings when settings button is clicked
        public void changeSettings(object sender, RoutedEventArgs e)
        {
            if (!isInSettings)
            {
                // change to settings
                Menu.Visibility = Visibility.Collapsed;
                SettingsGrid.Visibility = Visibility.Visible;

                // change settings state to true
                isInSettings = true;
            }
            else
            {
                // change to menu
                SettingsGrid.Visibility = Visibility.Collapsed;
                Menu.Visibility = Visibility.Visible;

                // change settings state to false
                isInSettings = false;
            }
        }






        // ********** methods to draw the dog **********
        // method to draw the dog's head based on its direction
        private void DrawHead()
        {
            // remove previous head if it exists
            if (dogHead != null)
                GameGrid.Children.Remove(dogHead);


            // create new head image
            dogHead = new Image();
            dogHead.Source = new BitmapImage(new Uri("assets/figure/head.png", UriKind.Relative));
            dogHead.Stretch = Stretch.Fill;


            // rotate head based on direction
            RotateTransform rotate = new RotateTransform();
            if (direction == "right")
                rotate.Angle = 0;

            if (direction == "down")
                rotate.Angle = 90;

            if (direction == "left")
                rotate.Angle = 180;

            if (direction == "up")
                rotate.Angle = 270;

            // rotate head
            dogHead.RenderTransform = rotate;
            dogHead.RenderTransformOrigin = new Point(0.5, 0.5);


            // position head in grid
            Grid.SetRow(dogHead, headRow);
            Grid.SetColumn(dogHead, headCol);


            // add head to grid
            GameGrid.Children.Add(dogHead);
        }



        // method to draw the dog's body based on its direction and position
        private void DrawBody()
        {
            // remove old body parts
            foreach (var img in bodyImages)
                GameGrid.Children.Remove(img);

            bodyImages.Clear();


            // add body parts to head
            for (int i = 1; i < dog.Count; i++)
            {
                if (i + 1 != dog.Count)
                {
                    // add body part to dog
                    Image body = new Image();
                    Point prev = dog[i - 1];
                    Point current = dog[i];
                    Point next = dog[i + 1];


                    // create rotate transform to rotate body part based on direction
                    RotateTransform rotate = new RotateTransform();


                    // calculate direction of body parts
                    double dx1 = prev.X - current.X;
                    double dy1 = prev.Y - current.Y;

                    double dx2 = next.X - current.X;
                    double dy2 = next.Y - current.Y;


                    // ***** choose body part *****
                    // straight horizontal
                    if (prev.X == next.X)
                    {
                        body.Source = new BitmapImage(new Uri("assets/figure/body.png", UriKind.Relative));
                        body.Stretch = Stretch.Fill;
                        rotate.Angle = 0;
                    }

                    // straight vertical
                    else if (prev.Y == next.Y)
                    {
                        body.Source = new BitmapImage(new Uri("assets/figure/body.png", UriKind.Relative));
                        body.Stretch = Stretch.Fill;
                        rotate.Angle = 90;
                    }

                    // curve
                    else
                    {
                        body.Source = new BitmapImage(new Uri("assets/figure/body_curve.png", UriKind.Relative));
                        body.Stretch = Stretch.Fill;

                        // bottom + right
                        if ((dx1 == -1 && dy2 == -1) || (dx2 == -1 && dy1 == -1))
                            rotate.Angle = 270;

                        // bottom + left
                        else if ((dx1 == -1 && dy2 == 1) || (dx2 == -1 && dy1 == 1))
                            rotate.Angle = 0;

                        // top + left
                        else if ((dx1 == 1 && dy2 == 1) || (dx2 == 1 && dy1 == 1))
                            rotate.Angle = 90;

                        // top + right
                        else
                            rotate.Angle = 180;
                    }


                    // rotate body part
                    body.RenderTransform = rotate;
                    body.RenderTransformOrigin = new Point(0.5, 0.5);


                    // position body part in grid
                    Grid.SetRow(body, (int)dog[i].X);
                    Grid.SetColumn(body, (int)dog[i].Y);


                    // add body part to grid
                    GameGrid.Children.Add(body);
                    bodyImages.Add(body);
                }

                else
                {
                    // add back part to end of the dog
                    Image back = new Image();
                    back.Source = new BitmapImage(
                        new Uri($"assets/figure/back.png", UriKind.Relative));
                    back.Stretch = Stretch.Fill;


                    // rotate back part based on direction of last two body parts
                    Point tail = dog[dog.Count - 1];
                    Point beforeTail = dog[dog.Count - 2];


                    // create rotate transform to rotate back part based on direction
                    RotateTransform rotate = new RotateTransform();


                    // calculate direction of back part
                    if (tail.X == beforeTail.X)
                    {
                        if (tail.Y < beforeTail.Y)
                            rotate.Angle = 0;
                        else
                            rotate.Angle = 180;
                    }
                    else
                    {
                        if (tail.X < beforeTail.X)
                            rotate.Angle = 90;
                        else
                            rotate.Angle = 270;
                    }


                    // rotate back part
                    back.RenderTransform = rotate;
                    back.RenderTransformOrigin = new Point(0.5, 0.5);


                    // position back part in grid
                    Grid.SetRow(back, (int)dog[dog.Count - 1].X);
                    Grid.SetColumn(back, (int)dog[dog.Count - 1].Y);


                    // add back part to grid
                    GameGrid.Children.Add(back);
                    bodyImages.Add(back);
                }
            }
        }



        // ********** handle key presses to change direction **********
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // change direction based on key press
            switch (e.Key)
            {
                case Key.W:
                    if (direction != "down")
                        direction = "up";
                    break;
                case Key.S:
                    if (direction != "up")
                        direction = "down";
                    break;
                case Key.A:
                    if (direction != "right")
                        direction = "left";
                    break;
                case Key.D:
                    if (direction != "left")
                        direction = "right";
                    break;
            }
        }






        // ********** game methods **********
        // method to build the game field based on user input
        private void buildGamefield()
        {
            // clear previous grid
            GameGrid.Children.Clear();
            GameGrid.RowDefinitions.Clear();
            GameGrid.ColumnDefinitions.Clear();


            // create rows and columns based on user input
            for (int row = 0; row < size; row++)
            {
                GameGrid.RowDefinitions.Add(new RowDefinition());
                GameGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }


            // create checkerboard pattern
            for (int r = 0; r < size; r++)
            {
                // create columns based on user input
                for (int c = 0; c < size; c++)
                {
                    Border border = new Border();


                    // determine color based on row and column index
                    if (r == 0 || r == 2 || r == 4 || r == 6 || r == 8 || r == 10 || r == 12 || r == 14 || r == 16 || r == 18 || r == 20)
                        // here columns are light green and the rest are green
                        if (c == 0 || c == 2 || c == 4 || c == 6 || c == 8 || c == 10 || c == 12 || c == 14 || c == 16 || c == 18 || c == 20)
                            border.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#a7d948") ?? Brushes.Transparent);
                        else
                            border.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#8ecc39") ?? Brushes.Transparent);

                    else if (r == 1 || r == 3 || r == 5 || r == 7 || r == 9 || r == 11 || r == 13 || r == 15 || r == 17 || r == 19)
                        // here columns are light green and the rest are green
                        if (c == 1 || c == 3 || c == 5 || c == 7 || c == 9 || c == 11 || c == 13 || c == 15 || c == 17 || c == 19)
                            border.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#a7d948") ?? Brushes.Transparent);
                        else
                            border.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#8ecc39") ?? Brushes.Transparent);


                    // set the border's position in the grid
                    Grid.SetColumn(border, c);
                    Grid.SetRow(border, r);
                    GameGrid.Children.Add(border);
                }
            }
        }



        // method to start the game loop when the start button is clicked
        public void startGameloop(object sender, RoutedEventArgs e)
        {
            //reset game state
            ResetGame();


            // draw the dog head
            DrawHead();
            dog.Add(new Point(headRow, headCol));
            dog.Add(new Point(headRow, headCol - 1));


            // start game loop
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(interval);
            gameTimer.Tick += GameLoop;
            gameTimer.Start();


            // hide menu
            MenuGrid.Visibility = Visibility.Collapsed;
        }



        // method to reset the game state
        private void ResetGame()
        {
            // remove all body images
            foreach (var img in bodyImages)
                GameGrid.Children.Remove(img);

            bodyImages.Clear();


            // remove all bone images
            foreach (var img in boneImages)
                GameGrid.Children.Remove(img);

            boneImages.Clear();
            bonePositions.Clear();
            points = 0;


            // remove head
            if (dogHead != null)
                GameGrid.Children.Remove(dogHead);


            // reset lists
            dog.Clear();
            bodyTypes.Clear();


            // reset position
            headRow = 5;
            headCol = 1;


            // reset direction
            direction = "right";


            // reset bone
            bonePositioned = false;
        }



        // game loop method
        private void GameLoop(object? sender, EventArgs e)
        {
            // calculate new head position based on direction
            int newRow = headRow;
            int newCol = headCol;


            // update head position based on direction
            switch (direction)
            {
                case "up":
                    newRow--;
                    break;
                case "down":
                    newRow++;
                    break;
                case "left":
                    newCol--;
                    break;
                case "right":
                    newCol++;
                    break;
            }


            // add new head position to dog list and body types list
            dog.Insert(0, new Point(newRow, newCol));
            bodyTypes.Insert(0, random.Next(1, 5));
            headRow = newRow;
            headCol = newCol;


            // check if dog collides with wall
            if (newRow < 0 || newRow >= size || newCol < 0 || newCol >= size)
            {
                // stop the game and show menu
                MenuGrid.Visibility = Visibility.Visible;
                gameTimer?.Stop();
                return;
            }


            // collide with self
            for (int i = 1; i < dog.Count - 1; i++)
            {
                if (dog[i].X == newRow && dog[i].Y == newCol)
                {
                    // stop the game and show menu
                    MenuGrid.Visibility = Visibility.Visible;
                    gameTimer?.Stop();
                    return;
                }
            }



            // eat bone
            int idx = bonePositions.FindIndex(p => p.X == headRow && p.Y == headCol);
            if (idx >= 0)
            {
                GameGrid.Children.Remove(boneImages[idx]);
                boneImages.RemoveAt(idx);
                bonePositions.RemoveAt(idx);

                points++;
                PointsText.Text = points.ToString();

                bonePositioned = false;
            }
            else
            {
                dog.RemoveAt(dog.Count - 1);
                bodyTypes.RemoveAt(bodyTypes.Count - 1);
            }


            // create bones
            if (!bonePositioned)
            {
                for (int i = 0; i < bones - boneImages.Count; i++)
                {
                    // generate random position for bone
                    boneRow = random.Next(0, size);
                    boneCol = random.Next(0, size);


                    // check if bone spawns on dog and if it does generate new position until it doesn't
                    while (boneRow == headRow && boneCol == headCol || dog.Any(p => p.X == boneRow && p.Y == boneCol) || bonePositions.Any(p => p.X == boneRow && p.Y == boneCol))
                    {
                        boneRow = random.Next(0, size);
                        boneCol = random.Next(0, size);
                    }


                    // position bone
                    Image bone = new Image();
                    bone.Source = new BitmapImage(new Uri("assets/img/bone.png", UriKind.Relative));
                    bonePositions.Add(new Point(boneRow, boneCol));
                    boneImages.Add(bone);


                    // set bone to stretch to fill cell
                    Grid.SetRow(bone, boneRow);
                    Grid.SetColumn(bone, boneCol);
                    GameGrid.Children.Add(bone);
                }


                // set bone var to true
                bonePositioned = true;
            }


            // create dog
            DrawHead();
            DrawBody();
        }
    }

}