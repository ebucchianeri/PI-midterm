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

    let mutable mediacolore = new Color()
    let mutable cw2v = new Drawing2D.Matrix()
    let mutable cv2w = new Drawing2D.Matrix()

    member this.Mediacolore
        with get() = mediacolore
        and set(v) = mediacolore <- v

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
    let f = 0
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


    let mutable img : Image = null
    let mutable bitmap : Bitmap = null
    let mutable ratio = 0.
    let mutable selection = new Rectangle()
    let basep = 50 // la base del grande pixel e' 50
    let mutable altezzap = 0 // altezza del grande pixel da calcolare
    let mutable mediaR = 0 // uso per calcolare la media dei colori
    let mutable mediaG = 0
    let mutable mediaB = 0
    let mutable mediaA = 0

    let tile = new ResizeArray<CImage>() // Immagini prelevate dalla cartella selezionata


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
        // posso scegliere se fare un photomosaic o un visualizzatore tramite 2 bottoni che dopo la scelta vengono rimossi
        let bf1 = new Bottone(Text="Visualizzatore",Left=0,Top=50,Width=100,Height=25,BackColor=Color.Blue)
        let bf2 = new Bottone(Text="Photomosaic",Left=102,Top=50,Width=100,Height=25,BackColor=Color.Blue)
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


        bf1.MouseDown.Add(fun _ -> 
            // inizializzo il visualizzatore
            if selectedFolder = null then
                this.LWCControls.RemoveAt(0)
                this.LWCControls.RemoveAt(0)
                let fd = new FolderBrowserDialog()
                fd.ShowNewFolderButton <- false
                if fd.ShowDialog() = DialogResult.OK then
                    selectedFolder <- fd.SelectedPath

                    let f = Directory.GetFiles(selectedFolder)
                    
                    numeroimmagini <- f.GetLength(0)-1
                    for y in 0 .. 1 .. (numeroimmagini) do
                        let i = loadImage f.[y]
                        neededi.Add(new CImage(Image=i,Left=0,Top=0,Height=i.Height,Width=i.Width))

                    let rad = Math.Sqrt(float(numeroimmagini)+float(1))
                    let elementixriga = int((numeroimmagini+1)/int(rad))
                    let mutable sommap = 0
                    let mutable altezzamax = 0

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
            )

        bf2.MouseDown.Add(fun _ -> 
            // inizializzo il photo mosaic
            if selectedFolder = null then
                this.LWCControls.RemoveAt(0)
                this.LWCControls.RemoveAt(0)
                if img = null then
                    // Scelgo l'immagine 
                    let od = new OpenFileDialog()
                    od.CheckFileExists <- true
                    if od.ShowDialog() = DialogResult.OK then
                
                        img <- Bitmap.FromFile(od.FileName)
                        bitmap <- new Bitmap(img)
                        ratio <- float(img.Width) / float(img.Height)
                        // Seleziono la cartella da quale prendere le immagini per il mosaico
                        if selectedFolder = null then
                            let fd = new FolderBrowserDialog()
                            fd.ShowNewFolderButton <- false
                            if fd.ShowDialog() = DialogResult.OK then
                                selectedFolder <- fd.SelectedPath

                            let f = Directory.GetFiles(selectedFolder)
                    
                            numeroimmagini <- f.GetLength(0)-1

                            for y in 0 .. 1 .. (numeroimmagini-1) do
                                let i = loadImage f.[y]
                                //i.Dispose()
                                use bi = new Bitmap(i)
                                let n = bi.Height*bi.Width
                                mediaA <- 0
                                mediaR <- 0
                                mediaG <- 0
                                mediaR <- 0
                                for px in 0 .. 1 .. bi.Width-1 do
                                    for py in 0 .. 1 .. bi.Height-1 do
                                        let c = bi.GetPixel(px,py)
                                        let c1 = System.Convert.ToInt32(c.R)
                                        let c2 = System.Convert.ToInt32(c.G)
                                        let c3 = System.Convert.ToInt32(c.B)
                                        let a = System.Convert.ToInt32(c.A)
                                        mediaR <- mediaR + c1
                                        mediaG <- mediaG + c2
                                        mediaB <- mediaB + c3
                                        mediaA <- mediaA + a

                                mediaR <- mediaR/n
                                mediaG <- mediaG/n
                                mediaB <- mediaB/n
                                mediaA <- mediaA/n
                                let c = Color.FromArgb(mediaA,mediaR,mediaG,mediaB)
                                printfn "tile %d colore %s" y (c.ToString())
                        
                                bi.Dispose() 
                                System.GC.Collect()
                                neededi.Add(new CImage(Image=i,Left=0,Top=0,Height=i.Height,Width=i.Width,Mediacolore=c))

                                this.Focus() |> ignore
                                this.Invalidate()
                )

        this.LWCControls.Add(bf1)
        this.LWCControls.Add(bf2)

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
        // mi interessa il mousemove solo se non sto usando il photomosaic
        if img = null then
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
        // mi interessa il mouseup solo se non sto usando il photomosaic
          if img = null then
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
            if img = null then    
                let l = transformPoint v2w e.Location
                neededi.Reverse() 
                let indice = neededi |> Seq.tryFindIndex (fun c -> c.Correlate(new Point(l.X - c.Left, l.Y - c.Top)))
                
                match indice with
                | Some idx ->
                    captured <- true
                    // Cerco di implementare lo Z ORDER, quello selezionato lo devo disegnare per ultimo
                    capturedImage <- neededi.[idx]
                    neededi.Remove(neededi.[idx]) |> ignore
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

        // funzione che prepara le CImage contenute in un array chiamato tile per il photomosaic,
        // l'array tile contiene la disposizione delle immagini da disegnare
    member this.aggiornaphotomosaic() =
        let mutable x =0
        let mutable y =0
        
        // n di pixel in un grande pixel
        let n = altezzap*basep
        
        // Per ogni pixelone calcolo la media del colore che e' la media delle 4 componenti del colore, uso 4 variabili mutabili
        for y in 0 .. altezzap .. bitmap.Height-1 do
            for x in 0 .. basep .. bitmap.Width-1 do

                let mutable px = x
                let mutable py = y
                mediaR <- 0
                mediaB <- 0
                mediaG <- 0
                mediaA <- 0
                for px in px .. 1 .. px+basep-1 do
                    for py in py .. 1 .. py+altezzap-1 do
                        if( px < bitmap.Width && py < bitmap.Height) then
                            let c = bitmap.GetPixel(px,py)
                            let c1 = System.Convert.ToInt32(c.R)
                            let c2 = System.Convert.ToInt32(c.G)
                            let c3 = System.Convert.ToInt32(c.B)
                            let a = System.Convert.ToInt32(c.A)
                            // sommo tutte le componeti di ogni pixel
                            mediaR <- mediaR + c1
                            mediaG <- mediaG + c2
                            mediaB <- mediaB + c3
                            mediaA <- mediaA + a

                // ne faccio la media
                mediaR <- mediaR/n
                mediaG <- mediaG/n
                mediaB <- mediaB/n
                mediaA <- mediaA/n
                //il colore medio e' dato da mediaA mediaR mediaG mediaB 
                let c = Color.FromArgb(mediaA,mediaR,mediaG,mediaB)

                let mutable cA = abs(System.Convert.ToInt32(neededi.[0].Mediacolore.A) - mediaA)
                let mutable cR = abs(System.Convert.ToInt32(neededi.[0].Mediacolore.R) - mediaR)
                let mutable cG = abs(System.Convert.ToInt32(neededi.[0].Mediacolore.G) - mediaG)
                let mutable cB = abs(System.Convert.ToInt32(neededi.[0].Mediacolore.B) - mediaB)
                let mutable t = 0
                // Adesso devo cercare l'immagine che tra quelle caricate dalla cartella ha il colore medio piu' simile al colore medio del pixelone
                for i in 0 .. 1 .. neededi.Count-1 do
                        let mutable cfA = abs(System.Convert.ToInt32(neededi.[i].Mediacolore.A) - mediaA)
                        let mutable cfR = abs(System.Convert.ToInt32(neededi.[i].Mediacolore.R) - mediaR)
                        let mutable cfG = abs(System.Convert.ToInt32(neededi.[i].Mediacolore.G) - mediaG)
                        let mutable cfB = abs(System.Convert.ToInt32(neededi.[i].Mediacolore.B) - mediaB)
                        if cfR < cR && cfG < cG && cfB < cB && cfA <= cA then
                            cA <- abs(System.Convert.ToInt32(neededi.[i].Mediacolore.A) - mediaA)
                            cR <- abs(System.Convert.ToInt32(neededi.[i].Mediacolore.R) - mediaR)
                            cG <- abs(System.Convert.ToInt32(neededi.[i].Mediacolore.G) - mediaG)
                            cB <- abs(System.Convert.ToInt32(neededi.[i].Mediacolore.B) - mediaB)
                            t <- i

                // L'immagine col colore medio piu' vicino e' quella di indice t
                tile.Add(new CImage(Image=neededi.[t].Image,Left=x,Top=y,Width=neededi.[t].Width,Height=neededi.[t].Height))
    
    override this.OnResize _ = this.Invalidate()

    override this.OnPaint e =
        let g = e.Graphics
        let s = g.Save()
        g.Transform <- w2v
        // Se ho una immagine nella variabile img allora sto usando come photomosaic
        if img <> null then
            // Al resize ho una modifica della dimensione
            let r = float(this.Width) / float(this.Height)
            let w, h = 
                if r < ratio then
                    (this.Width, int(float(this.Width) / ratio))
                else
                    (int(float(this.Height) * ratio), this.Height)
            
            
            // Aggiorno la vista e la bitmap in caso di resize
            g.DrawImage(img, new Rectangle(0, 0, w,h))
            bitmap <- new Bitmap(img,new Size(w,h))

            altezzap <- int(float(basep) / r)
            
            // Rimuovo i vecchi elementi e ricalcolo
            tile.Clear()
            this.aggiornaphotomosaic()
            
            tile.ToArray() |> Seq.iter ( fun c -> 
                let s1 = g.Save()
               
                let srcRect = new Rectangle(0,0,c.Width,c.Height)
                // Rettangolo di destinazione
                let destRect = new Rectangle(c.Left,c.Top,basep,altezzap)
                let units = GraphicsUnit.Pixel
                g.DrawImage(c.Image,destRect,srcRect,units)
                // ripristino il contesto grafico per disegnare una nuova immagine
                g.Restore(s1)
                )
        else
            // Se invece ho selezionato una cartella sto richiedendo il visualizzatore di immagini
            if selectedFolder <> null then
                
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


let f = new Form(Text="Esercizio4", TopMost=true)
f.Show()

let c = new Visualizza(Dock=DockStyle.Fill)
f.Controls.Add(c)
f.Invalidate()