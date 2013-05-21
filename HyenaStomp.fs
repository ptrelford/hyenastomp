namespace HyenaStomp

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media
open System.Windows.Shapes

type App() as this = 
    inherit Application()
    let width, height = 512., 512.
    let colors = [
        Colors.Red; Colors.Red; Colors.Red; 
        Colors.Blue; Colors.Blue; Colors.Blue; 
        Colors.Green; Colors.Green; Colors.Green; 
        Colors.Yellow; 
        Colors.Orange
        ]
    let colors = 
        [
        237, 86, 59
        232, 71, 61
        222, 64, 55
        72, 86, 87
        65, 84, 94
        62, 76, 89
        67, 81, 81
        73, 87, 72
        81, 106, 67
        254, 241, 4
        254, 103, 56
        ]
        |> List.map (fun (r,g,b) -> Color.FromArgb(255uy, byte r, byte g, byte b))
    
    let count = 29

    let mutable xs = []
    let mutable rs = []

    let drawing () =
        let canvas = Canvas(Width=width,Height=height)

        let left = Canvas(Width=width/2.0,Height=height)
        let right = Canvas(Width=width/2.0,Height=height)
        let top = Canvas(Width=width,Height=height/2.0)
        let bottom = Canvas(Width=width,Height=height/2.0)
        let dx = width / float count
        let dy = width / float count        
        for i = 0 to 14 do
            let rect = Rectangle(Width=dx-1.0,Height=height)
            Canvas.SetLeft(rect, -dx/2.0 + 0.5 + float i * dx)
            let x = (1+(i*4)) % colors.Length
            rect.Fill <- SolidColorBrush(colors.[x])
            xs <- (x,rect) :: xs
            rs <- rect :: rs
            right.Children.Add <| rect
            let rect = Rectangle(Width=width, Height=dy - 1.)
            Canvas.SetTop(rect, dy/2.0 + height/2.0 - float (1+i) * dy)
            let x = (2+(i*4)) % colors.Length
            rect.Fill <- SolidColorBrush(colors.[x])
            xs <- (x,rect) ::xs
            rs <- rect :: rs
            top.Children.Add <| rect
            let rect = Rectangle(Width=dx-1.0,Height=height)
            Canvas.SetLeft(rect, width/2.0 - float (1+i) * dx)
            let x = (3+(i*4)) % colors.Length
            rect.Fill <- SolidColorBrush(colors.[x])
            xs <- (x,rect) :: xs
            rs <- rect :: rs
            left.Children.Add <| rect
            let rect = Rectangle(Width=width,Height=dy - 1.)
            Canvas.SetTop(rect, 0.5 + float i * dy)
            let x = (4+(i*4)) % colors.Length
            rect.Fill <- SolidColorBrush(colors.[x])
            xs <- (x,rect) :: xs
            rs <- rect :: rs
            bottom.Children.Add <| rect

        let createClip (x1,y1) (x2,y2) (x3,y3) =
            let geometry = PathGeometry()
            let figure = PathFigure(IsClosed=true)
            figure.StartPoint <- Point(x1,y1)
            figure.Segments.Add <| LineSegment(Point=Point(x2,y2))
            figure.Segments.Add <| LineSegment(Point=Point(x3,y3))
            geometry.Figures.Add <| figure
            geometry

        left.Clip <- createClip (-0.5,0.) (width/2. - 0.5,height/2.) (-0.5,height-1.)
        Canvas.SetLeft(left, 0.0)
        canvas.Children.Add(left)

        right.Clip <- createClip (width/2.0 + 0.5,0.) (0.5,height/2.) (width/2.+0.5,height-1.)
        Canvas.SetLeft(right, width/2.0 - dx/2.0)
        Canvas.SetTop(right, -dy/2.)
        canvas.Children.Add(right)

        top.Clip <- createClip (0.,-0.5) (width-1.,-0.5) (width/2.,height/2.-0.5) 
        Canvas.SetTop(top, -dy/2.)
        Canvas.SetLeft(top, -dx/2.)
        canvas.Children.Add(top)

        bottom.Clip <- createClip (0.0,height/2.0+0.5) (width/2.0,0.5) (width,height/2.+0.5)
        Canvas.SetTop(bottom, height/2.0)
        canvas.Children.Add(bottom)
        
        Canvas.SetTop(canvas,-dy/2.)
        Canvas.SetLeft(canvas,-dx/2.)

        let f () = async {   
            do! Async.Sleep 500                   
            let i = ref 0
            while true do
                //do! Async.Sleep 500
                for step = 0 to 180-1 do
                    do! Async.Sleep(1+int (200.* cos ((float step / 180.) * (System.Math.PI / 2.0))))
                    for (x,rect) in xs do
                        let x = (x + !i) % colors.Length 
                        rect.Fill <- SolidColorBrush(colors.[x])
                    incr i
                //do! Async.Sleep 500
                for step = 180-1 downto 0 do
                    do! Async.Sleep(1+int (200.* cos ((float step / 180.) * (System.Math.PI / 2.0))))
                    for (x,rect) in xs do
                        let x = (x + !i) % colors.Length 
                        rect.Fill <- SolidColorBrush(colors.[x])
                    i := !i + colors.Length-1 
            }
        canvas, f
    
    do  this.Startup.AddHandler(fun o e -> 
            let canvas, f = drawing()
            this.RootVisual <- canvas
            f () |> Async.StartImmediate
            )