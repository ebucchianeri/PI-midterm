#load "LightWeightControls.fsx"

open System
open System.Windows.Forms
open System.Drawing
open LightWeightControls
open System.IO

type ActionType =
| Up
| Down
| Left
| Right
| RotateLeft
| RotateRight
| ZoomIn
| ZoomOut

// BOTTONE LWC con testo
type Bottone() =
  inherit LWCControl()
  
  let mutable pressed = false
  let mutable text = null

  member this.Text
        with get() = text
        and set(v) = text <- v
    
  override this.OnMouseDown e =
    base.OnMouseDown e
    pressed <- true
    this.Invalidate()

  override this.OnMouseUp e =
    base.OnMouseUp e
    pressed <- false
    this.Invalidate()

  override this.OnPaint e =
    base.OnPaint e
    let g = e.Graphics
    let sz = this.ClientSize
    let lgc = if pressed then
                 SystemColors.ControlDark
              else
                 SystemColors.ControlLight
    let wc = if pressed then
                 SystemColors.ControlDarkDark
             else
                 SystemColors.ControlLightLight
    let dgc = if pressed then
                 SystemColors.ControlLight
              else
                 SystemColors.ControlDark
    let bc = if pressed then
                 SystemColors.ControlLightLight
             else
                 SystemColors.ControlDarkDark
    use plgc = new Pen(lgc)
    use pwc = new Pen(wc)

    g.DrawLine(plgc, 0, 0, sz.Width, 0)
    g.DrawLine(plgc, 0, 0, 0, sz.Height)
    g.DrawLine(pwc, 1, 1, sz.Width, 1)
    g.DrawLine(pwc, 1, 1, 1, sz.Height)
    g.DrawLine(new Pen(dgc), 1, sz.Height - 2, sz.Width, sz.Height - 2)
    g.DrawLine(new Pen(dgc), sz.Width - 2, 1, sz.Width - 2, sz.Height)
    g.DrawLine(new Pen(bc), 0, sz.Height - 1, sz.Width, sz.Height - 1)
    g.DrawLine(new Pen(bc), sz.Width - 1, 0, sz.Width - 1, sz.Height)

    let font = new Font(FontFamily.GenericSerif, 8.0F)
    let txtsz = g.MeasureString(this.Text, font)
    g.DrawString(this.Text, font, Brushes.Black, ((single(sz.Width) - txtsz.Width) / 2.f) + (if pressed then 2.f else 0.f), ((single(sz.Height) - txtsz.Height) / 2.f) + (if pressed then 2.f else 0.f))

// Controllo LW che contiene una immagine
type CImage() =
    inherit LWCControl()
    let mutable image : Image = null
    let mutable clicked = false
    let mutable s = false
    let mutable cw2v = new Drawing2D.Matrix()
    let mutable cv2w = new Drawing2D.Matrix()

    member this.Clicked
        with get() = clicked
        and set(v) = clicked <- v

    member this.Image
        with get() = image
        and set(v) = image <- v

    override this.OnPaint e =
        let g = e.Graphics
        let q = g.Save()

        g.Transform <- cw2v
        // Rettangolo di origine
        let srcRect = new Rectangle(0,0,this.Width,this.Height)
        // Rettangolo di destinazione
        let destRect = new Rectangle(this.Left,this.Top,this.Width,this.Height)
        let units = GraphicsUnit.Pixel
        // Disegno l'immagine
        g.DrawImage(this.Image,destRect,srcRect,units)
        if clicked = true then
            g.FillRectangle(Brushes.Red,this.Left,this.Top,this.Width,2)
            g.FillRectangle(Brushes.Red,this.Left,this.Top,2,this.Height)
        g.Restore(q)



    override this.OnKeyDown e =
        let cx, cy = this.ClientSize.Width / 2, this.ClientSize.Height / 2
        match e.KeyData with
        | Keys.W -> 
          this.DoAction ActionType.Up 
        | Keys.S ->
          this.DoAction ActionType.Down 
        | Keys.A ->
          this.DoAction ActionType.Left 
        | Keys.D ->
          this.DoAction ActionType.Right 
        | Keys.Q ->
          this.DoAction ActionType.RotateLeft 
        | Keys.E ->
          this.DoAction ActionType.RotateRight
        | Keys.Z ->
          this.DoAction ActionType.ZoomIn 
        | Keys.X ->
          this.DoAction ActionType.ZoomOut 
        | _ -> ()

    member this.DoAction (act:ActionType) =
        let cx, cy = this.ClientSize.Width / 2, this.ClientSize.Height / 2
        match act with
        | ActionType.Up -> 
            cw2v.Translate(0.f, 10.f, Drawing2D.MatrixOrder.Append)
            cv2w.Translate(0.f, -10.f)
            this.Invalidate()
        | ActionType.Down ->
            cw2v.Translate(0.f, -10.f, Drawing2D.MatrixOrder.Append)
            cv2w.Translate(0.f, 10.f)
            this.Invalidate()
        | ActionType.Left ->
            cw2v.Translate(10.f, 0.f, Drawing2D.MatrixOrder.Append)
            cv2w.Translate(-10.f, 0.f)
            this.Invalidate()
        | ActionType.Right ->
            cw2v.Translate(-10.f, 0.f, Drawing2D.MatrixOrder.Append)
            cv2w.Translate(10.f, 0.f)
            this.Invalidate()
        | ActionType.RotateLeft ->
            cw2v.Translate(single(-cx), single(-cy), Drawing2D.MatrixOrder.Append)
            cw2v.Rotate(-10.f, Drawing2D.MatrixOrder.Append)
            cw2v.Translate(single(cx), single(cy), Drawing2D.MatrixOrder.Append)
            cv2w.Translate(single(cx), single(cy))
            cv2w.Rotate(10.f)
            cv2w.Translate(single(-cx), single(-cy))
            this.Invalidate()
        | ActionType.RotateRight ->
            cw2v.Translate(single(-cx), single(-cy), Drawing2D.MatrixOrder.Append)
            cw2v.Rotate(10.f, Drawing2D.MatrixOrder.Append)
            cw2v.Translate(single(cx), single(cy), Drawing2D.MatrixOrder.Append)
            cv2w.Translate(single(cx), single(cy))
            cv2w.Rotate(-10.f)
            cv2w.Translate(single(-cx), single(-cy))
            this.Invalidate()
        | ActionType.ZoomIn ->
            cw2v.Scale(1.1f, 1.1f, Drawing2D.MatrixOrder.Append)
            cv2w.Scale(1.f/1.1f, 1.f/1.1f)
            this.Invalidate()
        | ActionType.ZoomOut ->
            cw2v.Scale(1.f/1.1f, 1.f/1.1f, Drawing2D.MatrixOrder.Append)
            cv2w.Scale(1.1f, 1.1f)
            this.Invalidate()



type Visualizza() as this =
    inherit LWCPanel()

    let w2v = new Drawing2D.Matrix()
    let v2w = new Drawing2D.Matrix()

    let neededi = new ResizeArray<CImage>() // Immagini prelevate dalla cartella selezionata

    let mutable numeroimmagini = 0 // Numero di immagini nella cartella
    let mutable selectedFolder : string = null // Cartella dalla quale prelevare le immagini

    // Mi serve per fare il drag&drop
    let mutable captured = false
    let mutable capturedImage = new CImage()

    // Per calcolare la velocita' iniziale del possibile lancio
    let mutable lascio = false // Impostata a true nel mouseUp, serve per calcolare lo spostamento del mouse dopo il mouseUp e quindi salvare
                                // le coordinate del mouse dopo un tick del timer, se lo spostamento e' significativo avro' un lancio
    let mutable lanciabile = new CImage() // Immagine che deve essere lanciata

    let mutable lancio = false // Indica la possibilita' di un lancio, impostata a true dal mouseUp
    let mutable offset = new Point()
    let mutable selected = false // Ho selezionato una immagine
    let mutable selectedImage = new CImage() // Immagine selezionata
    // Due punti per calcolare il lancio dell'immagine
    let mutable puntorilascio1 = new Point()
    let mutable puntorilascio2 = new Point()

    let loadImage x = Image.FromFile(x)

    let transformPoint (m:Drawing2D.Matrix) (p:Point) =
        let pa = [| p |]
        m.TransformPoints(pa)
        pa.[0]


    let timer = new Timer(Interval=100) 
    let tempi =  new ResizeArray<int>() // Array di interi per i lanci delle immagini

    // Dati per il lancio, per adesso sono fissi
    let mutable a = -0.1f
    let mutable vi = 10.f

    let mutable action = None

    // funzione di interpolazione
    let attrito t (p0:int) a vi =
        let a1 = a/2.f
        let tt = t*t
        a1*tt + vi*t + float32(p0)

    // funzione che anima l'immagine modificando la proprieta' Top
    let animaTop (p:LWCControl,t0,t1,p0,p1,a,v,f) =
        tempi.Add(0)
        let l = tempi |> Seq.length
        let i = l - 1
        tempi.[i] <- 0
        let a = a
        let vi = v
        timer.Tick.Add(fun _ -> 
            if ( float32(tempi.[i] + 10) <= t1-t0 ) then
                tempi.[i] <- tempi.[i] + 10
                let x = f (float32(tempi.[i])) p0 a vi

                p.Top <- int(x)
                printfn "%d %d" (tempi.[i]) (int(x))
                this.Invalidate()
            )  

    // funzione che anima l'immagine modificando la proprieta' Left
    let animaLeft (p:LWCControl,t0,t1,p0,p1,a,v,f) =
        tempi.Add(0)
        let l = tempi |> Seq.length
        let i = l - 1
        tempi.[i] <- 0
        let a = a
        let vi = v
        timer.Tick.Add(fun _ -> 
            if ( float32(tempi.[i] + 10) <= t1-t0 ) then
                tempi.[i] <- tempi.[i] + 10
                let x = f (float32(tempi.[i])) p0 a vi

                p.Left <- int(x)
                printfn "%d %d" (tempi.[i]) (int(x))
                this.Invalidate()
            )  
    // funzione presi in ingresso due punti, calcolando la distanza tra i due, decide se lanciare in una direzione l'immagine.
    // la direzione e' decisa  dai due punti in input
    let scarto (p1:Point) (p2:Point) = 
        let sx, sy = abs(p2.X - p1.X), abs(p2.Y - p1.Y)
        if sx >= 10  then
            // DEVO CALCOLARE LA DIREZIONE! per ora li mando in direzioni prefissate
            let dx = p2.X-p1.X
            if dx > 0 then
              // deve andare verso destra
                a <- -0.1f
                vi <- 10.f
                let tf = -vi/a
                let pf = attrito tf lanciabile.Left a vi
                animaLeft(lanciabile,float32(1000),float32(1000)+tf,lanciabile.Left,int(pf),a,vi,attrito)
            else
                a <- 0.1f
                vi <- - 10.f
                let tf = -vi/a
                let pf = attrito tf lanciabile.Left a vi
                animaLeft(lanciabile,float32(1000),float32(1000)+tf,lanciabile.Left,int(pf),a,vi,attrito)
        if sy >= 10 then
            let dy = p2.Y-p1.Y
            if dy > 0 then
                a <- -0.1f
                vi <-  10.f
                
                let tf = -vi/a
                let pf = attrito tf lanciabile.Top a vi
                animaTop(lanciabile,float32(1000),float32(1000)+tf,lanciabile.Top,int(pf),a,vi,attrito)
            else
            //  deve andare verso l'alto
                a <- 0.1f
                vi <- -10.f
                let tf = -vi/a
                let pf = attrito tf lanciabile.Top a vi
                animaTop(lanciabile,float32(1000),float32(1000)+tf,lanciabile.Top,int(pf),a,vi,attrito)
            lancio <- false

    do
        this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true)
        // AGGIUNGO I BOTTONI e le loro funzionalita'
        let bup = new Bottone(Text="UP",Left=0,Top=0,Width=50,Height=25,BackColor=Color.Blue)
        let bdown = new Bottone(Text="DOWN",Left=52,Top=0,Width=50,Height=25,BackColor=Color.Blue)
        let bleft = new Bottone(Text="LEFT",Left=104,Top=0,Width=50,Height=25,BackColor=Color.Blue)
        let bright = new Bottone(Text="RIGHT",Left=156,Top=0,Width=50,Height=25,BackColor=Color.Blue)
        let bzi = new Bottone(Text="ZOOMIN",Left=208,Top=0,Width=60,Height=25,BackColor=Color.Blue)
        let bzo = new Bottone(Text="ZOOMOUT",Left=270,Top=0,Width=70,Height=25,BackColor=Color.Blue)
        let brl = new Bottone(Text="ROT.L",Left=342,Top=0,Width=50,Height=25,BackColor=Color.Blue)
        let brr = new Bottone(Text="ROT.R",Left=394,Top=0,Width=50,Height=25,BackColor=Color.Blue)

        timer.Tick.Add(fun _ ->
          
          match action with
            | Some a -> 
                // la trasformazione la applico al pannello ma anche a tutte le immagini caricate
                this.DoAction a
                neededi.ToArray() |> Seq.iter ( fun c -> c.DoAction a)
            | None -> ()
        )
        timer.Start()
        bup.MouseDown.Add(fun _ -> 
            if selected = true then
                selectedImage.DoAction ActionType.Up
            else 
                action <- Some ActionType.Up;
            )
        bup.MouseUp.Add(fun _ -> action <- None;  )

        bdown.MouseDown.Add(fun _ -> 
            if selected = true then
                selectedImage.DoAction ActionType.Down
            else 
                action <- Some ActionType.Down;
            )
        bdown.MouseUp.Add(fun _ -> action <- None;  )

        bleft.MouseDown.Add(fun _ -> 
            if selected = true then
                selectedImage.DoAction ActionType.Left
            else 
                action <- Some ActionType.Left;)

        bleft.MouseUp.Add(fun _ -> action <- None;  )

        bright.MouseDown.Add(fun _ -> 
            if selected = true then
                selectedImage.DoAction ActionType.Right
            else 
                action <- Some ActionType.Right; )

        bright.MouseUp.Add(fun _ -> action <- None;  )

        bzi.MouseDown.Add(fun _ -> 
            if selected = true then
                selectedImage.DoAction ActionType.ZoomIn
            else 
                action <- Some ActionType.ZoomIn)
        bzi.MouseUp.Add(fun _ -> action <- None;  )

        bzo.MouseDown.Add(fun _ -> 
            if selected = true then
                selectedImage.DoAction ActionType.ZoomOut
            else 
                action <- Some ActionType.ZoomOut)
        bzo.MouseUp.Add(fun _ -> action <- None;  )


        brl.MouseDown.Add(fun _ -> 
            if selected = true then
                selectedImage.DoAction ActionType.RotateLeft
            else 
                action <- Some ActionType.RotateLeft)
        brl.MouseUp.Add(fun _ -> action <- None;  )

        brr.MouseDown.Add(fun _ -> 
            if selected = true then
                selectedImage.DoAction ActionType.RotateRight
            else 
                action <- Some ActionType.RotateRight)
        brr.MouseUp.Add(fun _ -> action <- None;  )

        this.LWCControls.Add(bup)
        this.LWCControls.Add(bdown)
        this.LWCControls.Add(bleft)
        this.LWCControls.Add(bright)
        this.LWCControls.Add(bzi)
        this.LWCControls.Add(bzo)
        this.LWCControls.Add(brl)
        this.LWCControls.Add(brr)

        timer.Tick.Add( fun c ->
            // Se ho appena lasciato una immagine allora mi salvo le coordinate del mouse dopo 100ms in puntorilascio2 e chiamo la funzione scarto
                if lascio = true then
                    printfn "punto rilascio2: x e y %d %d" puntorilascio2.X puntorilascio2.Y
                    scarto puntorilascio1 puntorilascio2
                    lascio <- false
        )
    
    override this.OnMouseMove e =
        base.OnMouseMove e
        if not base.MouseHandled then
            // Se ho selezionato qualcosa
            let l = transformPoint v2w e.Location
            if lascio = true then
                //printfn "move %d %d" l.X l.Y
                puntorilascio2 <- new Point(l.X,l.Y)
                // Qui potrei calcolare la "VELOCITa' INIZIALE!" conosco entrambi i punti
            if captured = true then
                              
                capturedImage.Left <- l.X - offset.X 
                capturedImage.Top <- l.Y - offset.Y
                
                //printfn "spostato %d %d" capturedImage.Left capturedImage.Top
                this.Invalidate()
                
                


    override this.OnMouseUp e =
        base.OnMouseUp e 
        if not base.MouseHandled then
            // Se captured era true, ovvero stavo usando drag&drop su qualche immagine e adesso ho l'up, allora captured diventa false
            if captured = true then
                captured <- false
                // devo controllare di poter lanciare l'immagine. Se
                if lancio = false then
                    lancio <- true
                    lanciabile <- capturedImage
                // Vado a calcolare lo spostamento del mouse 
                lascio <- true     
            let l = transformPoint v2w e.Location
            puntorilascio1 <- new Point(l.X,l.Y)
            printfn "rilascio drag&drop %d %d" l.X l.Y


    override this.OnMouseDown e =
        base.OnMouseDown e
        if not base.MouseHandled then
        // Non ho ancora selezionato la cartella siamo nella fase di inizializzazione
            if selectedFolder = null then
                let fd = new FolderBrowserDialog()
                fd.ShowNewFolderButton <- false
                if fd.ShowDialog() = DialogResult.OK then
                    selectedFolder <- fd.SelectedPath

                    let f = Directory.GetFiles(selectedFolder)
                    // Carico le immagini e le metto nell'array
                    numeroimmagini <- f.GetLength(0)-1
                    for y in 0 .. 1 .. (numeroimmagini) do
                        let i = loadImage f.[y]
                        neededi.Add(new CImage(Image=i,Left=0,Top=0,Height=i.Height,Width=i.Width))
                    // Calcolo quante metterne per riga e per colonna
                    let rad = Math.Sqrt(float(numeroimmagini)+float(1))
                    let elementixriga = int((numeroimmagini+1)/int(rad))
                    let mutable sommap = 0
                    let mutable altezzamax = 0

                    // Per ogni immagine devo ricavare la sua posizione e scriverla comesua  proprieta'
                    for y in 0 .. 1 .. numeroimmagini do
                        if  y%elementixriga = 0 then
                            // Calcolo la proprieta' TOP delle immagini
                             
                            let temp = altezzamax
                            for i in y+0 .. 1 .. y+elementixriga do
                                if( i <= numeroimmagini ) then
                                    neededi.[i].Top <- altezzamax
                            altezzamax <- 0
                            for i in y+0 .. 1 .. y+elementixriga do
                                if( i <= numeroimmagini ) then
                                    altezzamax <- Math.Max(altezzamax,neededi.[i].Height)
                            altezzamax <- altezzamax + temp + 1

                            // Calcolo la proprieta' LEFT delle immagini 
                            sommap <- 0
                            neededi.[y].Left <- sommap
                            sommap <- sommap + neededi.[y].Width + 1
                        else
                            neededi.[y].Left <- sommap
                            sommap <- sommap + neededi.[y].Width + 1
                        //printfn "%d %d %d" y (neededi.[y].Left) (neededi.[y].Top)

                    this.Invalidate()
                    this.Focus() |> ignore
            else 
            // Ho gia' selezionato la cartella           
                let l = transformPoint v2w e.Location
                neededi.Reverse() 
                let indice = neededi |> Seq.tryFindIndex (fun c -> c.Correlate(new Point(l.X - c.Left, l.Y - c.Top)))
                
                match indice with
                | Some idx ->
                    captured <- true
                    // Cerco di implementare lo Z ORDER, quello selezionato lo devo disegnare per ultimo
                    capturedImage <- neededi.[idx]
                    neededi.Remove(neededi.[idx]) |> ignore
                    // lo inserisco in 0 e poi faccio reverse, diventera' l'ultimo
                    neededi.Insert(0,capturedImage)
                    // Offset del click
                    offset <- new Point((l.X-capturedImage.Left),l.Y-capturedImage.Top)
                    // Se ho gia' qualcosa di selezionato
                    if selected = true then
                        // Se clicco sulla stessa foto:
                        if selectedImage.Image.Equals(neededi.[0].Image) then
                            selectedImage.Invalidate()
                                
                        // Se clicco su due foto diverse:
                        else
                            selectedImage.Clicked <- false
                            selectedImage <- neededi.[0] 
                            neededi.[0].Clicked <- true
                            this.Invalidate()
                    else
                        // Se non avevo niente di selezionato
                        selected <- true
                        selectedImage <- neededi.[0] 
                        neededi.[0].Clicked <- true
                        this.Invalidate()
                | None -> 
                    selected <- false
                    selectedImage.Clicked <- false
                    this.Invalidate()
                neededi.Reverse() 


    override this.OnPaint e =
        let g = e.Graphics
        // Se ho selezionato almeno una cartella col MouseDown
        if selectedFolder <> null then
            let s = g.Save()
            g.Transform <- w2v
            let rad = Math.Sqrt(float(numeroimmagini)+float(1))
            let elementixriga = int((numeroimmagini+1)/int(rad))
            let mutable sommap = 0
            let mutable altezzamax = 0
            
            // Per ogni CImage presente in neededi la disegno chiamando la sua paint
            neededi.ToArray() |> Seq.iter (
                fun c ->
                c.Parent <- this
                // salvo il contesto grafico che ogni immagine puo' modificare
                let s1 = g.Save()
                
                if c.Image = selectedImage.Image then
                    printfn "DrawImage at %d %d" c.Left c.Top
                let r = new Region(new RectangleF(single(c.Left),single(c.Top), single(c.Width + 1), single(c.Height + 1)))        
                g.Clip <- r
                let evt = new PaintEventArgs(g, new Rectangle(c.Left, c.Top, c.Width, c.Height))
                c.OnPaintBackground(evt)
                c.OnPaint(evt)
                // ripristino il contesto grafico per disegnare una nuova immagine
                g.Restore(s1)
            )
            // rispristino il contesto grafico iniziale per disegnare i controlli
            g.Restore(s)
            base.OnPaint e

    member this.DoAction (act:ActionType) = 
            let cx, cy = this.ClientSize.Width / 2, this.ClientSize.Height / 2
            match act with
            | ActionType.Up -> 
                w2v.Translate(0.f, 10.f, Drawing2D.MatrixOrder.Append)
                v2w.Translate(0.f, -10.f)
                this.Invalidate()
            | ActionType.Down ->
                w2v.Translate(0.f, -10.f, Drawing2D.MatrixOrder.Append)
                v2w.Translate(0.f, 10.f)
                this.Invalidate()
            | ActionType.Left ->
                w2v.Translate(10.f, 0.f, Drawing2D.MatrixOrder.Append)
                v2w.Translate(-10.f, 0.f)
                this.Invalidate()
            | ActionType.Right ->
                w2v.Translate(-10.f, 0.f, Drawing2D.MatrixOrder.Append)
                v2w.Translate(10.f, 0.f)
                this.Invalidate()
            | ActionType.RotateLeft ->
                w2v.Translate(single(-cx), single(-cy), Drawing2D.MatrixOrder.Append)
                w2v.Rotate(-10.f, Drawing2D.MatrixOrder.Append)
                w2v.Translate(single(cx), single(cy), Drawing2D.MatrixOrder.Append)
                v2w.Translate(single(cx), single(cy))
                v2w.Rotate(10.f)
                v2w.Translate(single(-cx), single(-cy))
                this.Invalidate()
            | ActionType.RotateRight ->
                w2v.Translate(single(-cx), single(-cy), Drawing2D.MatrixOrder.Append)
                w2v.Rotate(10.f, Drawing2D.MatrixOrder.Append)
                w2v.Translate(single(cx), single(cy), Drawing2D.MatrixOrder.Append)
                v2w.Translate(single(cx), single(cy))
                v2w.Rotate(-10.f)
                v2w.Translate(single(-cx), single(-cy))
                this.Invalidate()
            | ActionType.ZoomIn ->
                w2v.Scale(1.1f, 1.1f, Drawing2D.MatrixOrder.Append)
                v2w.Scale(1.f/1.1f, 1.f/1.1f)
                this.Invalidate()
            | ActionType.ZoomOut ->
                w2v.Scale(1.f/1.1f, 1.f/1.1f, Drawing2D.MatrixOrder.Append)
                v2w.Scale(1.1f, 1.1f)
                this.Invalidate()


let f = new Form(Text="Esercizio3", TopMost=true)
f.Show()

let c = new Visualizza(Dock=DockStyle.Fill)
f.Controls.Add(c)