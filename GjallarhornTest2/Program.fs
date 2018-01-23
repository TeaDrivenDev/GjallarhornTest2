namespace GjallarhornTest2

open System

open FsXaml
open Gjallarhorn
open Gjallarhorn.Bindable
open Gjallarhorn.Bindable.Framework
open Gjallarhorn.Wpf

type MainWindow = XAML<"MainWindow.xaml">


module Program =

    let reverse (s : string) = s |> Seq.rev |> String.Concat

    type Msg =
        | Input of string
        | Add of string

    type Thing =
        {
            Input : string
            Items : string list
        }
    with
        static member Default = { Input = ""; Items = [] }
    
    let update msg (thing : Thing) =
        match msg with
        | Input input -> { thing with Input = input }
        | Add item -> { thing with Input = ""; Items = item :: thing.Items }

    [<CLIMutable>]
    type ViewModel =
        {
            Original : string
            Reverse : string
            Add : VmCmd<string>
            Items : string list
        }

    let d = { Original = ""; Reverse = ""; Add = Vm.cmd ""; Items = [] }

    
    let bindToSource _nav source (model : ISignal<Thing>) : IObservable<Msg> list =
        let input =
            model
            |> Signal.map (fun m -> m.Input)
            |> Bind.Explicit.twoWay source (nameof <@ d.Original @>)

        model
        |> Signal.map (fun m -> reverse m.Input)
        |> Bind.Explicit.oneWay source (nameof <@ d.Reverse @>)

        model
        |> Signal.map (fun m -> m.Items)
        |> Bind.Explicit.oneWay source (nameof <@ d.Items @>)

        let add = Bind.Explicit.createCommand (nameof <@ d.Add @>) source

        [
            Signal.map id input |> Observable.map Input
            add |> Observable.map (fun _ -> model.Value.Input |> reverse |> Add)
        ]

    let applicationCore = Framework.application Thing.Default update (Component.fromExplicit bindToSource) Nav.empty

    [<STAThread>]
    [<EntryPoint>]
    let main _ =
        Framework.RunApplication (Navigation.singleViewFromWindow MainWindow, applicationCore)

        1
