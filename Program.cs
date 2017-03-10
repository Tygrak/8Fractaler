using System;
using System.Threading;
using System.Drawing;
using System.Collections.Generic;
using OpenTK.Input;
using OpenTK;
//using OpenTK.Graphics.OpenGL;
using SharpCanvas;

public class Program{
    public static void Main(string[] args){
        Fractal g = new Fractal();
    }
}

public class Fractal{
    Canvas canvas;
    float position = 96;
    public List<Square> squares = new List<Square>();
    public Thread mathThread;
    Bitmap bmp;
    int iteration = 1;
    int num = 3;
    double aspectRatioWidth = 1;
    float timer = 3f;
    bool drawing = false;
    Vector2d mandPos = new Vector2d();
    double currZoom = 6.0;
    int mode = 1;
    bool julia = false;
    Vector2d juliaPos = new Vector2d();
    int colorMode = 1;
    public Fractal(){
        int width = 900;
        int height = 900;
        #if !DEBUG
        Console.WriteLine("Choose window size in pixels. ");
        Console.WriteLine("Width: ");
        int.TryParse(Console.ReadLine(), out width);
        Console.WriteLine("Height: ");
        int.TryParse(Console.ReadLine(), out height);
        #endif
        if(width != height){
            aspectRatioWidth = ((double)width)/height;
        }
        canvas = new Canvas(width, height);
        canvas.ImageCanvasMode();
        /*width = 7680;
        height = 4320;
        aspectRatioWidth = ((double)width)/height;
        canvas.ImageCanvasMode();
        canvas.imageCanvas.image = new Bitmap(width, height);*/
        canvas.keyActions.Add(new KeyAction(Key.I, new Action(canvas.MakeImage), KeyActionMode.keyReleased));
        canvas.drawingEnabled = false;
        bmp = canvas.imageCanvas.image;
        canvas.updateAction = new Action(OnUpdate);
        //MandelbrotSet(0, 0, 4.2);
        //MandelbrotSet(-0.77, 0.09, 0.12);
        //MandelbrotSet(-0.745, 0.095, 0.01);
        //MandelbrotSet(-0.74665, 0.09465, 0.0001);
        //MandelbrotSet(-0.74665, 0.09465, 0.00001);
        //JuliaSet(0, -0.8, 0, 0, 3.6);
        //JuliaSet(-0.4, 0.6, 0, 0, 3.6);
        //JuliaSet(-1.4, 0, 0, 0, 3.6);
        //JuliaSet(-1.4, 0, -0.01, -0.015, 0.01);
        //BurningShip(-0.4, -0.4, 2.8);
        //BurningShip(-1.75, -0.04, 0.1);
        //BurningShipJuliaSet(-1.25, -0.38, 0, 0, 4.2);
        //BurningShipJuliaSet(-1.75, -0.05, 0, 0, 4.2);
        //canvas.drawingEnabled = true;
        drawing = true;
        VerhulstProcess(2,4);
        //MandelbrotSet(mandPos.X, mandPos.Y, currZoom);
        drawing = false;
        //canvas.imageCanvas.image.Save("7680×4320.png");
        canvas.drawingEnabled = true;
        canvas.mouseActions.Add(new MouseAction(MouseButton.Left, new Action(zoomClick)));
        canvas.mouseActions.Add(new MouseAction(MouseButton.Right, new Action(zoomOutClick)));
        canvas.keyActions.Add(new KeyAction(Key.M, new Action(changeMode), KeyActionMode.keyReleased));
        canvas.keyActions.Add(new KeyAction(Key.J, new Action(toJulia), KeyActionMode.keyReleased));
        canvas.keyActions.Add(new KeyAction(Key.C, new Action(changeColorMode), KeyActionMode.keyReleased));
    }

    public void CantorSet(){
        List<Square> newSquares = new List<Square>();
        for (int i = 0; i < squares.Count; i++){
            float xPos = squares[i].Position.X;
            float size = 100f/num;
            for (int j = 0; j < 3; j++){
                if(j%3==1){
                } else{
                    newSquares.Add(new Square(xPos, position, xPos+size, position+3));
                }
                xPos += size;
            }
        }
        squares = new List<Square>(newSquares);
        for (int i = 0; i < squares.Count; i++){
            canvas.Draw(squares[i]);
        }
        num *= 3;
        position -= 6;
    }

    public void VerhulstProcess(double lambda, double zoom){
        DateTime d = DateTime.Now;
        double min = lambda-(zoom*aspectRatioWidth/2);
        double max = lambda+(zoom*aspectRatioWidth/2);
        //int maxIterations = 1100;
        for (int i = 0; i < bmp.Width; i++){
            double startLambda = MathExtensions.Map(i, 0, bmp.Width, min, max);
            //Console.WriteLine(startLambda);
            double x = 0.5;
            for (int j = 0; j < 1200; j++){
                x = startLambda * x * (1-x);
                if(j>1000){
                    int yPos = (int) Math.Round(MathExtensions.Map(x, 0, 1, 0, bmp.Height));
                    Color c = Color.White;
                    bmp.SetPixel(i, yPos, c);
                }
            }
        }
        Console.WriteLine("Done in: " + (DateTime.Now - d).TotalSeconds.ToString() + " seconds.");
    }

    public void MandelbrotSet(double posR, double posI, double zoom){
        DateTime d = DateTime.Now;
        double minR = posR-(zoom*aspectRatioWidth/2);
        double maxR = posR+(zoom*aspectRatioWidth/2);
        double minI = posI-zoom/2;
        double maxI = posI+zoom/2;
        int maxIterations = 1000;
        for (int x = 0; x < bmp.Width; x++){
            double starta = MathExtensions.Map(x, 0, bmp.Width, minR, maxR);
            //Console.WriteLine(x + " : " + starta.ToString());
            for (int y = 0; y < bmp.Height; y++){
                double startb = MathExtensions.Map(y, 0, bmp.Height, minI, maxI);
                double a = starta;
                double b = startb;
                double n = 0;
                int i = 0;
                while(n<2 && i < maxIterations){
                    double a2 = (a*a - b*b);
                    double b2 = (2*a*b);
                    i++;
                    a = a2 + starta;
                    b = b2 + startb;
                    n = a + b;
                }
                Color c = Color.Black;
                if(i == maxIterations){
                    c = Color.Black;
                } else{
                    c = colorFromIterations(i, maxIterations);
                }
                bmp.SetPixel(x, y, c);
            }
        }
        Console.WriteLine("Done in: " + (DateTime.Now - d).TotalSeconds.ToString() + " seconds.");
    }

    public void JuliaSet(double mandR, double mandI, double posR, double posI, double zoom){
        DateTime d = DateTime.Now;
        double minR = posR-(zoom*aspectRatioWidth/2);
        double maxR = posR+(zoom*aspectRatioWidth/2);
        double minI = posI-zoom/2;
        double maxI = posI+zoom/2;
        int maxIterations = 1000;
        for (int x = 0; x < bmp.Width; x++){
            double starta = MathExtensions.Map(x, 0, bmp.Width, minR, maxR);
            //Console.WriteLine(x + " : " + starta.ToString());
            for (int y = 0; y < bmp.Height; y++){
                double startb = MathExtensions.Map(y, 0, bmp.Height, minI, maxI);
                double a = starta;
                double b = startb;
                double n = 0;
                int i = 0;
                while(n<2 && i < maxIterations){
                    double a2 = (a*a - b*b);
                    double b2 = (2*a*b);
                    i++;
                    a = a2 + mandR;
                    b = b2 + mandI;
                    n = a + b;
                }
                Color c = Color.Black;
                if(i == maxIterations){
                    c = Color.Black;
                } else{
                    c = colorFromIterations(i, maxIterations);
                }
                bmp.SetPixel(x, y, c);
            }
        }
        Console.WriteLine("Done in: " + (DateTime.Now - d).TotalSeconds.ToString() + " seconds.");
    }

    public void BurningShip(double posR, double posI, double zoom){
        DateTime d = DateTime.Now;
        double minR = posR-(zoom*aspectRatioWidth/2);
        double maxR = posR+(zoom*aspectRatioWidth/2);
        double minI = posI-zoom/2;
        double maxI = posI+zoom/2;
        int maxIterations = 1000;
        for (int x = 0; x < bmp.Width; x++){
            double starta = MathExtensions.Map(x, 0, bmp.Width, minR, maxR);
            //Console.WriteLine(x + " : " + starta.ToString());
            for (int y = 0; y < bmp.Height; y++){
                double startb = MathExtensions.Map(y, 0, bmp.Height, minI, maxI);
                double a = starta;
                double b = startb;
                double n = 0;
                int i = 0;
                while(n<2 && i < maxIterations){
                    a = Math.Abs(a);
                    b = Math.Abs(b);
                    double a2 = (a*a - b*b);
                    double b2 = (2*a*b);
                    i++;
                    a = a2 + starta;
                    b = b2 + startb;
                    n = a + b;
                }
                Color c = Color.Black;
                if(i == maxIterations){
                    c = Color.Black;
                } else{
                    c = colorFromIterations(i, maxIterations);
                }
                bmp.SetPixel(x, y, c);
            }
        }
        bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
        Console.WriteLine("Done in: " + (DateTime.Now - d).TotalSeconds.ToString() + " seconds.");
    }

    public void BurningShipJuliaSet(double mandR, double mandI, double posR, double posI, double zoom){
        DateTime d = DateTime.Now;
        double minR = posR-(zoom*aspectRatioWidth/2);
        double maxR = posR+(zoom*aspectRatioWidth/2);
        double minI = posI-zoom/2;
        double maxI = posI+zoom/2;
        int maxIterations = 1000;
        for (int x = 0; x < bmp.Width; x++){
            double starta = MathExtensions.Map(x, 0, bmp.Width, minR, maxR);
            //Console.WriteLine(x + " : " + starta.ToString());
            for (int y = 0; y < bmp.Height; y++){
                double startb = MathExtensions.Map(y, 0, bmp.Height, minI, maxI);
                double a = starta;
                double b = startb;
                double n = 0;
                int i = 0;
                while(n<2 && i < maxIterations){
                    a = Math.Abs(a);
                    b = Math.Abs(b);
                    double a2 = (a*a - b*b);
                    double b2 = (2*a*b);
                    i++;
                    a = a2 + mandR;
                    b = b2 + mandI;
                    n = a + b;
                }
                Color c = Color.Black;
                if(i == maxIterations){
                    c = Color.Black;
                } else{
                    c = colorFromIterations(i, maxIterations);
                }
                bmp.SetPixel(x, y, c);
            }
        }
        Console.WriteLine("Done in: " + (DateTime.Now - d).TotalSeconds.ToString() + " seconds.");
    }

    public void PerpendicularBurningShip(double posR, double posI, double zoom){
        DateTime d = DateTime.Now;
        double minR = posR-(zoom*aspectRatioWidth/2);
        double maxR = posR+(zoom*aspectRatioWidth/2);
        double minI = posI-zoom/2;
        double maxI = posI+zoom/2;
        int maxIterations = 1000;
        for (int x = 0; x < bmp.Width; x++){
            double starta = MathExtensions.Map(x, 0, bmp.Width, minR, maxR);
            //Console.WriteLine(x + " : " + starta.ToString());
            for (int y = 0; y < bmp.Height; y++){
                double startb = MathExtensions.Map(y, 0, bmp.Height, minI, maxI);
                double a = starta;
                double b = startb;
                double n = 0;
                int i = 0;
                while(n<2 && i < maxIterations){
                    b = Math.Abs(b);
                    double a2 = (a*a - b*b);
                    double b2 = (2*a*b);
                    i++;
                    a = a2 + starta;
                    b = b2 + startb;
                    n = a + b;
                }
                Color c = Color.Black;
                if(i == maxIterations){
                    c = Color.Black;
                } else{
                    c = colorFromIterations(i, maxIterations);
                }
                bmp.SetPixel(x, y, c);
            }
        }
        Console.WriteLine("Done in: " + (DateTime.Now - d).TotalSeconds.ToString() + " seconds.");
    }

    public void Tricorn(double posR, double posI, double zoom){
        DateTime d = DateTime.Now;
        double minR = posR-(zoom*aspectRatioWidth/2);
        double maxR = posR+(zoom*aspectRatioWidth/2);
        double minI = posI-zoom/2;
        double maxI = posI+zoom/2;
        int maxIterations = 1000;
        for (int x = 0; x < bmp.Width; x++){
            double starta = MathExtensions.Map(x, 0, bmp.Width, minR, maxR);
            //Console.WriteLine(x + " : " + starta.ToString());
            for (int y = 0; y < bmp.Height; y++){
                double startb = MathExtensions.Map(y, 0, bmp.Height, minI, maxI);
                double a = starta;
                double b = startb;
                double n = 0;
                int i = 0;
                while(n<2 && i < maxIterations){
                    b = -b;
                    double a2 = (a*a - b*b);
                    double b2 = (2*a*b);
                    i++;
                    a = a2 + starta;
                    b = b2 + startb;
                    n = a + b;
                }
                Color c = Color.Black;
                if(i == maxIterations){
                    c = Color.Black;
                } else{
                    c = colorFromIterations(i, maxIterations);
                }
                bmp.SetPixel(x, y, c);
            }
        }
        Console.WriteLine("Done in: " + (DateTime.Now - d).TotalSeconds.ToString() + " seconds.");
    }

    public void GlitchBrotSet(double posR, double posI, double zoom){
        DateTime d = DateTime.Now;
        double minR = posR-(zoom*aspectRatioWidth/2);
        double maxR = posR+(zoom*aspectRatioWidth/2);
        double minI = posI-zoom/2;
        double maxI = posI+zoom/2;
        int maxIterations = 1000;
        for (int x = 0; x < bmp.Width; x++){
            double starta = MathExtensions.Map(x, 0, bmp.Width, minR, maxR);
            //Console.WriteLine(x + " : " + starta.ToString());
            for (int y = 0; y < bmp.Height; y++){
                double startb = MathExtensions.Map(y, 0, bmp.Height, minI, maxI);
                double a = starta;
                double b = startb;
                double n = 0;
                int i = 0;
                while(n<2 && i < maxIterations){
                    double a2 = (a*a - b*b);
                    double b2 = (2*a*b);
                    i++;
                    a = a2 + a + starta;
                    b = b2 + b + startb;
                    n = a + b;
                }
                Color c = Color.Black;
                if(i == maxIterations){
                    c = Color.Black;
                } else{
                    c = colorFromIterations(i, maxIterations);
                }
                bmp.SetPixel(x, y, c);
            }
        }
        Console.WriteLine("Done in: " + (DateTime.Now - d).TotalSeconds.ToString() + " seconds.");
    }

    public Color colorFromIterationsV1(int iterations, int maxIterations){
        Color c = Color.Black;
        int colorR = (int) MathExtensions.Map(iterations, 0, maxIterations, 0, 765);
        if(colorR<64){
            c = Color.FromArgb(0, colorR/2, colorR/4);
        }else if(colorR<160){
            c = Color.FromArgb(0, 40-colorR/4, colorR/2);
        } else if(colorR<256){
            c = Color.FromArgb(0, 0, colorR);
        } else if(colorR<511){
            colorR -= 255;
            c = Color.FromArgb(colorR, 0, 255-colorR);
        } else if(colorR<766){
            colorR -= 510;
            c = Color.FromArgb(255-colorR, colorR, 0);
        }
        return c;
    }

    public Color colorFromIterationsV2(int iterations, int maxIterations){
        Color c = Color.Black;
        int colorR = (int) Math.Round(MathExtensions.Map(iterations, 0, maxIterations, 0, 1000));
        if(colorR<32){
            c = Color.FromArgb(0, colorR/4, colorR/4);
        } else if(colorR<64){
            c = Color.FromArgb(0, colorR/3, colorR/3);
        }else if(colorR<160){
            c = Color.FromArgb(0, 41-colorR/4, colorR/2);
        } else if(colorR<256){
            c = Color.FromArgb(0, 0, colorR);
        } else if(colorR<385){
            colorR = (int) Math.Round(MathExtensions.Map(colorR, 256, 385, 0, 255));
            c = Color.FromArgb(0, colorR, 255-colorR);
        } else if(colorR<511){
            colorR = (int) Math.Round(MathExtensions.Map(colorR, 385, 511, 0, 255));
            c = Color.FromArgb(colorR, 255-colorR, 0);
        } else if(colorR<639){
            colorR = (int) Math.Round(MathExtensions.Map(colorR, 511, 638, 0, 255));
            c = Color.FromArgb(255-colorR, 0, colorR);
        } else if(colorR<767){
            colorR = (int) Math.Round(MathExtensions.Map(colorR, 639, 766, 0, 255));
            c = Color.FromArgb(colorR/2, colorR, 255-colorR);
        } else if(colorR<895){
            colorR = (int) Math.Round(MathExtensions.Map(colorR, 767, 895, 0, 255));
            c = Color.FromArgb(colorR/2 + 127, 255-colorR, colorR/2);
        } else if(colorR<950){
            colorR = (int) Math.Round(MathExtensions.Map(colorR, 895, 950, 0, 255));
            c = Color.FromArgb(255-colorR, colorR/2, colorR/2 + 127);
        } else{
            colorR = (int) Math.Round(MathExtensions.Map(colorR, 950, 1000, 0, 255));
            c = Color.FromArgb(colorR, colorR/2+127, 255-colorR);
        }
        //Console.WriteLine(c.R + ", " + c.G + ", " + c.B + ", ");
        return c;
    }

    public Color colorFromIterationsR(int iterations, int maxIterations){
        Color c = Color.Black;
        int colorR = (int) MathExtensions.Map(iterations, 0, maxIterations, 0, 765);
        if(colorR<64){
            c = Color.FromArgb(colorR/4, colorR/2, 0);
        }else if(colorR<160){
            c = Color.FromArgb(colorR/2, 40-colorR/4, 0);
        } else if(colorR<256){
            c = Color.FromArgb(colorR, 0, 0);
        } else if(colorR<511){
            colorR -= 255;
            c = Color.FromArgb(255-colorR, colorR, 0);
        } else if(colorR<766){
            colorR -= 510;
            c = Color.FromArgb(0, 255-colorR, colorR);
        }
        return c;
    }

    public Color colorFromIterationsOld(int iterations, int maxIterations){
        Color c = Color.Black;
        int colorR = (int) MathExtensions.Map(iterations, 0, maxIterations, 0, 765);
        if(iterations <= 1){
        } else if(iterations < maxIterations/3){
            int div = (maxIterations/3) / iterations;
            c = Color.FromArgb(255/div, 0, 0);
        } else if(iterations < 2*maxIterations/3){
            int div = (2*maxIterations/3) / iterations;
            c = Color.FromArgb(255-255/div, 255/div, 0);
        } else{
            int div = (maxIterations) / iterations;
            c = Color.FromArgb(0, 255-255/div, 255/div);
        }
        return c;
    }

    public Color colorFromIterations(int iterations, int maxIterations){
        Color c = Color.Black;
        if(colorMode == 1){
            c = colorFromIterationsV2(iterations, maxIterations);
        } else if(colorMode == 2){
            c = colorFromIterationsV1(iterations, maxIterations);
        } else if(colorMode == 3){
            c = colorFromIterationsR(iterations, maxIterations);
        } else if(colorMode == 4){
            c = colorFromIterationsOld(iterations, maxIterations);
        }
        return c;
    }

    public void changeMode(){
        if(!drawing){
            drawing = true;
            julia = false;
            mode++;
            mode %= 6;
            if(mode == 0) mode = 1;
            mandPos = new Vector2d();
            currZoom = 6;
            drawFractal();
            drawing = false;
        }
    }

    public void toJulia(){
        if(!drawing){
            Console.WriteLine("Generating Julia Set from position: " + mandPos.X + ", " + mandPos.Y);
            drawing = true;
            julia = true;
            juliaPos = mandPos;
            mandPos = new Vector2d();
            currZoom = 6;
            drawFractal();
            drawing = false;
        }
    }

    public void changeColorMode(){
        if(!drawing){
            drawing = true;
            colorMode++;
            colorMode %= 5;
            if(colorMode == 0) colorMode = 1;
            drawFractal();
            drawing = false;
        }
    }

    public void zoomClick(){
        if(!drawing){
            Vector2i mousePos = canvas.MousePosition();
            if(mousePos.x > canvas.width || mousePos.x < 0 || mousePos.y > canvas.height || mousePos.y < 0){
                return;
            }
            if(mode == 2 && !julia){
            } else{
                mousePos.y = canvas.height - mousePos.y;
            }
            double mPosX = MathExtensions.Map(mousePos.x, 0, bmp.Width, mandPos.X - currZoom*aspectRatioWidth/2, mandPos.X + currZoom*aspectRatioWidth/2);
            double mPosY = MathExtensions.Map(mousePos.y, 0, bmp.Height, mandPos.Y - currZoom/2, mandPos.Y + currZoom/2);
            drawing = true;
            mandPos.X = mPosX;
            mandPos.Y = mPosY;
            currZoom /= 2;
            Console.WriteLine("Position: " + mPosX + " " + mPosY + " Zoom: " + currZoom);
            drawFractal();
            drawing = false;
        }
    }

    public void zoomOutClick(){
        if(!drawing){
            Vector2i mousePos = canvas.MousePosition();
            if(mousePos.x > canvas.width || mousePos.x < 0 || mousePos.y > canvas.height || mousePos.y < 0){
                return;
            }
            if(mode == 2 && !julia){
            } else{
                mousePos.y = canvas.height - mousePos.y;
            }
            double mPosX = MathExtensions.Map(mousePos.x, 0, bmp.Width, mandPos.X - currZoom*aspectRatioWidth/2, mandPos.X + currZoom*aspectRatioWidth/2);
            double mPosY = MathExtensions.Map(mousePos.y, 0, bmp.Height, mandPos.Y - currZoom/2, mandPos.Y + currZoom/2);
            drawing = true;
            mandPos.X = mPosX;
            mandPos.Y = mPosY;
            currZoom *= 2;
            Console.WriteLine("Position: " + mPosX + " " + mPosY + " Zoom: " + currZoom);
            drawFractal();
            drawing = false;
        }
    }

    public void drawFractal(){
        if(julia){
            if(mode == 1){
                JuliaSet(juliaPos.X, juliaPos.Y, mandPos.X, mandPos.Y, currZoom);
            } else if(mode == 2){
                BurningShipJuliaSet(juliaPos.X, juliaPos.Y, mandPos.X, mandPos.Y, currZoom);
            }
        } else{
            if(mode == 1){
                MandelbrotSet(mandPos.X, mandPos.Y, currZoom);
            } else if(mode == 2){
                BurningShip(mandPos.X, mandPos.Y, currZoom);
            } else if(mode == 3){
                PerpendicularBurningShip(mandPos.X, mandPos.Y, currZoom);
            } else if(mode == 4){
                Tricorn(mandPos.X, mandPos.Y, currZoom);
            } else if(mode == 5){
                GlitchBrotSet(mandPos.X, mandPos.Y, currZoom);
            }
        }
    }

    public void OnUpdate(){
        timer -= (float) canvas.DeltaTime;
        if(timer < 0){
            //CantorSet();
            timer = 3;
            iteration++;
        }
    }
}
