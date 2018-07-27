// Esercizio 2
// Un’animazione è l’evoluzione di un insieme di proprietà di oggetti grafici nel tempo (f(t) -> {pi = vi}). 
// In una ragionevole implementazione di questo modello si indicano i valori di una proprietà di un oggetto grafico a istanti dati (es t0 e t1) 
// e una funzione di interpolazione che generi i valori intermedi per la proprietà (ad esempio l’interpolazione lineare sarà p(t) = (t – t0)/(t1 – t0) *  (p1 - p0) + p0). 
// Si implementi un’applicazione che con l’uso di un singolo timer consenta di descrivere animazioni di lightweight controls secondo questo modello.
#load "LightWeightControls.fsx"

open System.Windows.Forms
open System.Drawing
open LightWeightControls

type Quadrato() = 
    inherit LWCControl()

    override this.OnPaint e =
        let g = e.Graphics
        g.FillRectangle(Brushes.Red, 0, 0, this.Width, this.Height)


type Cerchio() = 
    inherit LWCControl()

    override this.OnPaint e =
        let g = e.Graphics
        g.FillEllipse(Brushes.Green, 0, 0, this.Width, this.Height)

type PannelloAnimato() as this =
    inherit LWCPanel()
    let tempi =  new ResizeArray<int>()
    let timer = new Timer(Interval = 100)

    let a = 0.1f
    let vi = -10.f

    let interpolazione t0 t1 p0 p1 t = 
        let y = single((t - t0))/single((t1 - t0))
        printfn "y %e         %e              %e" y (single(t)) (single(t0))
        printfn " p0 %e" (single(p0))
        y * single(p1 - p0) + single(p0)

    
    let animaTop (p:LWCControl,t0,t1,p0,p1,f) =
        tempi.Add(0)
        let l = tempi |> Seq.length
        let i = l - 1
        tempi.[i] <- t0
        timer.Tick.Add(fun _ -> 
            if ( tempi.[i] + 100 <= t1 ) then
                tempi.[i] <- tempi.[i] + 100
                let x = f t0 t1 p0 p1 tempi.[i]
                if x >= single(p0) && x <= single(p1) then
                    p.Top <- int(x)
                printfn "%d %d" (tempi.[i]) (int(x))
                this.Invalidate()

            ) 


    let animaLeft (p:LWCControl,t0,t1,p0,p1,f) =
        tempi.Add(0)
        let l = tempi |> Seq.length
        let i = l - 1
        tempi.[i] <- t0
        timer.Tick.Add(fun _ -> 
            if ( tempi.[i] + 100 <= t1 ) then
                tempi.[i] <- tempi.[i] + 100
                let x = f t0 t1 p0 p1 tempi.[i]
                if x >= single(p0) && x <= single(p1) then
                    p.Left <- int(x)
                printfn "%d %d" (tempi.[i]) (int(x))
                this.Invalidate()
            ) 

    do
        this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true)
        
        let q1 = new Cerchio(Left=600,Top=600,Width=50,Height=50)
        let q2 = new Quadrato(Top=100,Width=50,Height=50)
        this.LWCControls.Add(q1)
        this.LWCControls.Add(q2)

        animaTop(this.LWCControls.[1],1000,2000,q2.Top,q2.Top+160,interpolazione)
        animaLeft(this.LWCControls.[1],1000,2000,q2.Left,q2.Left+160,interpolazione)
        
        

    
    override this.OnPaint e =    
        let g = e.Graphics
        g.DrawLine(Pens.Black,0,0,100,100)
        base.OnPaint e
    

    // Comando l'animazione con il tasto A e S
    override this.OnKeyDown e =
        match e.KeyData with
        | Keys.A -> 
            timer.Start() 
        | Keys.S ->
            if timer.Enabled then
                timer.Stop()
        | _ -> ()
        
       

let f = new Form(TopMost=true)
f.Size = new Size(350,350)
let p = new PannelloAnimato(Dock=DockStyle.Fill)

f.Controls.Add(p)
f.Show()