namespace GjallarhornTest2

open System

open FsXaml
open Gjallarhorn
open Gjallarhorn.Bindable
open Gjallarhorn.Bindable.Framework
open Gjallarhorn.Wpf

type MainWindow = XAML<"MainWindow.xaml">


module Program =

    let names =
        [
           "Alpheratz"
           "Ankaa"
           "Schedar"
           "Diphda"
           "Achernar"
           "Hamal"
           "Acamar"
           "Menkar"
           "Mirfac"
           "Aldebaran"
           "Rigel"
           "Capella"
           "Bellatrix"
           "Elnath"
           "Alnilam"
           "Betelgeuse"
           "Canopus"
           "Sirius"
           "Adhara"
           "Procyon"
           "Pollux"
           "Avior"
        ]

    type Msg =
        | Add of string

    type Thing =
        {
            Source : string list
            Target : string list
        }
    with
        static member Default = { Source = names; Target = [] }
    
    let update msg (thing : Thing) =
        match msg with
        | Add item ->
            if thing.Target |> List.contains item
            then thing
            else { thing with Target = (item :: thing.Target) |> List.sort }

    
    type ItemViewModel =
        {
            Name : string
            Self : ISignal<string>
        }


    [<CLIMutable>]
    type ViewModel =
        {
            Source : string list
            Add : VmCmd<string>
            Target : string list
        }

    let d = { Source = []; Add = Vm.cmd ""; Target = [] }

    let bindToSource _nav source (model : ISignal<Thing>) : IObservable<Msg> list =
        model
        |> Signal.map (fun m -> m.Source)
        |> Bind.Explicit.oneWay source (nameof <@ d.Source @>)

        model
        |> Signal.map (fun m -> m.Target)
        |> Bind.Explicit.oneWay source (nameof <@ d.Target @>)

        let add = Bind.Explicit.createCommandParam (nameof <@ d.Add @>) source

        [
            add |> Observable.map Add
        ]

    let applicationCore = Framework.application Thing.Default update (Component.fromExplicit bindToSource) Nav.empty

    [<STAThread>]
    [<EntryPoint>]
    let main _ =
        Framework.RunApplication (Navigation.singleViewFromWindow MainWindow, applicationCore)

        1
