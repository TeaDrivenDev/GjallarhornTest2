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
    
    type ItemMessage = UpdateItem

    [<CLIMutable>]
    type Item =
        {
            Name : string
            Top : float
            Left : float
        }

    let createItem name = { Name = name; Top = 200.; Left = 200. }

    [<CLIMutable>]
    type ItemViewModel =
        {
            XName : string
            Top : float
            Left : float
            Self : ISignal<Item>
        }

    let designSignal = createItem "" |> Signal.constant
    let itemd = { XName = ""; Top = 0.; Left = 0.; Self = designSignal }

    let itemComponent =
        let update msg itemModel =
            match msg with
            | UpdateItem -> itemModel

        Component.create<Item, unit, _>
            [
                <@ itemd.XName @> |> Bind.oneWay (fun item -> item.Name)
                <@ itemd.Top @> |> Bind.oneWay (fun item -> item.Top)
                <@ itemd.Left @> |> Bind.oneWay (fun item -> item.Left)
                <@ itemd.Self @> |> Bind.self
            ]
        |> Component.toSelfUpdating update

    type ModelMessage =
        | Add of string

    type Model =
        {
            Source : string list
            Target : Item list
        }
    with
        static member Default = { Source = names; Target = [] }

    let updateModel msg (model : Model) =
        match msg with
        | Add name ->
            if model.Target |> List.exists (fun item -> item.Name = name)
            then model
            else
                { model with
                    Target = (createItem name :: model.Target) |> List.sort }

    [<CLIMutable>]
    type ViewModel =
        {
            XSource : string list
            Add : VmCmd<string>
            XTarget : ItemViewModel list
        }

    let d = { XSource = []; Add = Vm.cmd ""; XTarget = [] }

    let bindToSource _nav source (model : ISignal<Model>) : IObservable<ModelMessage> list =
        model
        |> Signal.map (fun m -> m.Source)
        |> Bind.Explicit.oneWay source (nameof <@ d.XSource @>)

        model
        |> Signal.map (fun m -> m.Target)
        |> Bind.Explicit.oneWay source (nameof <@ d.XTarget @>)

        let add = Bind.Explicit.createCommandParam (nameof <@ d.Add @>) source

        [
            add |> Observable.map Add
        ]

    let applicationCore = Framework.application Model.Default updateModel (Component.fromExplicit bindToSource) Nav.empty

    [<STAThread>]
    [<EntryPoint>]
    let main _ =
        Framework.RunApplication (Navigation.singleViewFromWindow MainWindow, applicationCore)

        1
