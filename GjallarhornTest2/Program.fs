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

    type Thing = { Input : string }
    with
        static member Default = { Input = "" }
    
    let update msg (thing : Thing) =
        match msg with
        | Input input -> { thing with Input = input }

    [<CLIMutable>]
    type ViewModel = { Original : string; Reverse : string }

    let d = { Original = ""; Reverse = "a" }

    
    let bindToSource _nav source (model : ISignal<Thing>) : IObservable<Msg> list =
        let input =
            model
            |> Signal.map (fun m -> m.Input)
            |> Bind.Explicit.twoWay source (nameof <@ d.Original @>)

        model
        |> Signal.map (fun m -> reverse m.Input)
        |> Bind.Explicit.oneWay source (nameof <@ d.Reverse @>)

        [
            Signal.map id input
            |> Observable.map Input
        ]

    let applicationCore = Framework.application Thing.Default update (Component.fromExplicit bindToSource) Nav.empty

    [<STAThread>]
    [<EntryPoint>]
    let main _ =
        Framework.RunApplication (Navigation.singleViewFromWindow MainWindow, applicationCore)

        1
