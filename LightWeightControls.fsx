open System.Windows.Forms
open System.Drawing

type LWCControl() =
  let mutable x, y, w, h = 0, 0, 100, 100

  let mutable parent : Control = null

  let mutable backColor = Color.Transparent

  let mouseDown = new Event<MouseEventArgs>()
  let mouseUp = new Event<MouseEventArgs>()
  let mouseMove = new Event<MouseEventArgs>()
  let mouseEnter = new Event<MouseEventArgs>()
  let mouseLeave = new Event<MouseEventArgs>()
  let keyDown = new Event<KeyEventArgs>()

  [<CLIEvent>] member this.MouseDown = mouseDown.Publish
  [<CLIEvent>] member this.MouseUp = mouseUp.Publish
  [<CLIEvent>] member this.MouseMove = mouseMove.Publish
  [<CLIEvent>] member this.MouseEnter = mouseEnter.Publish
  [<CLIEvent>] member this.MouseLeave = mouseLeave.Publish
  [<CLIEvent>] member this.KeyDown = keyDown.Publish

  member this.BackColor
   with get() = backColor
   and set(v) = backColor <- v; this.Invalidate()

  member this.Parent
    with get() = parent
    and set(v) = parent <- v

  member this.Width 
    with get() = w
    and set(v) = w <- v

  member this.Height 
    with get() = h
    and set(v) = h <- v

  member this.Left 
    with get() = x
    and set(v) = x <- v

  member this.Top 
    with get() = y
    and set(v) = y <- v

  member this.ClientRectangle 
    with get() = new Rectangle(x, y, w, h)

  member this.ClientSize
    with get() = new Size(w, h)

  
  
  abstract OnKeyDown : KeyEventArgs -> unit 
  default this.OnKeyDown e = keyDown.Trigger(e)

  abstract OnMouseMove : MouseEventArgs -> unit 
  default this.OnMouseMove e = mouseMove.Trigger(e)

  abstract OnMouseUp : MouseEventArgs -> unit 
  default this.OnMouseUp e = mouseUp.Trigger(e) 

  abstract OnMouseDown : MouseEventArgs -> unit 
  default this.OnMouseDown e = mouseDown.Trigger(e)

  abstract OnMouseEnter : MouseEventArgs -> unit 
  default this.OnMouseEnter e = mouseEnter.Trigger(e) 

  abstract OnMouseLeave : MouseEventArgs -> unit 
  default this.OnMouseLeave e = mouseLeave.Trigger(e) 

  abstract OnPaintBackground : PaintEventArgs -> unit
  default this.OnPaintBackground e =
    if backColor <> Color.Transparent then
      let g = e.Graphics
      use bgc = new SolidBrush(backColor)
      g.FillRectangle(bgc, 0, 0, this.ClientSize.Width, this.ClientSize.Height)

  abstract OnPaint : PaintEventArgs -> unit
  default this.OnPaint _ = ()

  abstract Correlate : Point -> bool
  default this.Correlate p =
    p.X >= 0 && p.Y >= 0 && p.X < w && p.Y < h

  member this.Invalidate() =
    if parent <> null then
      let cr = this.ClientRectangle
      let r = new Rectangle(cr.Left, cr.Top, cr.Width + 1, cr.Height + 1)
      parent.Invalidate(r)

type LWCPanel()  =
  inherit Control()

  let controls = new ResizeArray<LWCControl>()
  let mutable hoverctl = None
  let mutable correlated = false
  let mutable capture = None

  let correlateMouse (e:MouseEventArgs) =
    let control = controls
                    |> Seq.tryFindIndex (fun c -> c.Correlate(new Point(e.X - c.Left, e.Y - c.Top )))
    match control with
    | Some idx -> 
        let cx, cy = controls.[idx].Left, controls.[idx].Top
        let evt = new MouseEventArgs(e.Button, e.Clicks, e.X - cx, e.Y - cy, e.Delta)
        correlated <- true
        Some(controls.[idx], evt)
    | None -> correlated <- false; None

  //member this.Controls = controls
  member this.LWCControls
    with get() = controls

  member this.MouseHandled with get() = correlated

  override this.OnMouseMove e =
    match correlateMouse e with
    | Some (c, evt) ->
        if capture.IsSome && capture.Value <> c then
          capture.Value.OnMouseMove(evt)
        if hoverctl.IsSome && hoverctl.Value <> c then 
          hoverctl.Value.OnMouseLeave(evt)
          c.OnMouseEnter(evt)
        if hoverctl.IsNone then 
          c.OnMouseEnter(evt)
        c.OnMouseMove(evt)
        hoverctl <- Some(c)
    | None -> 
        // FIXME: the evet shuld be generated...
        if capture.IsSome then capture.Value.OnMouseMove(e)
        if hoverctl.IsSome then hoverctl.Value.OnMouseLeave(e)
        hoverctl <- None

  override this.OnMouseUp e =
    match correlateMouse e with
    | Some (c, evt) -> 
      if capture.IsSome && capture.Value <> c then capture.Value.OnMouseUp(evt)
      c.OnMouseUp(evt)
    | None -> 
        // FIXME: the evet shuld be generated...
      if capture.IsSome then capture.Value.OnMouseUp(e)

    capture <- None

  override this.OnMouseDown e =
    match correlateMouse e with
    | Some (c, evt) -> 
      capture <- Some c
      c.OnMouseDown(evt)
    | None -> ()

  override this.OnPaint e =
    let g = e.Graphics
    controls.ToArray() |> Array.rev |> Seq.iter (
      fun c ->
        c.Parent <- this
        let s = g.Save()
        g.TranslateTransform(single(c.Left), single(c.Top))
        let r = new Region(new RectangleF(0.f, 0.f, single(c.Width + 1), single(c.Height + 1)))        
        g.Clip <- r
        let evt = new PaintEventArgs(g, new Rectangle(0, 0, c.Width, c.Height))
        c.OnPaintBackground(evt)
        c.OnPaint(evt)
        g.Restore(s)
    )
