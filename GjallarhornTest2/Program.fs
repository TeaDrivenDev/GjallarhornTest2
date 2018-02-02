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
    
    type Item =
        {
            Name : string
            Top : float
            Left : float
            Id : Guid // Used for identity later
        }

    let createItem name = { Name = name; Top = 200.; Left = 200. ; Id = Guid.NewGuid() }

    type ItemMessage = 
        | ChangeLeft of float
        | ChangeTop of float
    
    let updateItem msg (item : Item) =
        match msg with
        | ChangeLeft left -> { item with Left = left }
        | ChangeTop top -> { item with Top = top }

    [<CLIMutable>]
    type ItemViewModel =
        {
            Name : string
            Top : float
            Left : float            
        }
    let itemd = { Name = ""; Top = 0.; Left = 0. }


    let itemComponent =
        Component.create<Item, unit, ItemMessage>
            [
                <@ itemd.Name @> |> Bind.oneWay (fun item -> item.Name) 
                <@ itemd.Top @> |> Bind.twoWay (fun item -> item.Top) ChangeTop
                <@ itemd.Left @> |> Bind.twoWay (fun item -> item.Left) ChangeLeft
            ]        

    type Model =
        {
            Source : string list
            Target : Item list
        }
    with
        static member Default = { Source = names; Target = [] }

    type ModelMessage =
        | Add of string
        | ChangeItem of ItemMessage * Item


    let updateModel msg (model : Model) =
        
        // Using the Id to find/replace items instead of equality since more than one update 
        // happens in one pass (single dispatcher operation), without an update between
        let get item model = 
            model 
            |> List.find (fun i -> i.Id = item.Id)
        let replace old repl model = 
            model 
            |> List.map (fun i -> if i.Id <> old.Id then i else repl)

        match msg with
        | Add name ->
            if model.Target |> List.exists (fun item -> item.Name = name)
            then model
            else
                { model with
                    Target = (createItem name :: model.Target) |> List.sort }
        | ChangeItem (imsg,item) ->
            let orig = get item model.Target
            let updated = orig |> updateItem imsg 
            { model with 
                Target = replace orig updated model.Target }

    [<CLIMutable>]
    type ViewModel =
        {
            Source : string list
            Add : VmCmd<ModelMessage> // This should be the type of command you generate, not the param coming from XAML
            Target : Item list
        }

    let d = { Source = []; Add = Vm.cmd (Add ""); Target = [] }

    // This can be implicit - see below - if you prefer explicit, this will still work

    //let bindToSource (_nav : Dispatch<unit>) source (model : ISignal<Model>) : IObservable<ModelMessage> list =
    //    model
    //    |> Signal.map (fun m -> m.Source)
    //    |> Bind.Explicit.oneWay source (nameof <@ d.Source @>)

    //    let t = model |> Signal.map (fun m -> m.Target)
    //    let updateItem = Gjallarhorn.Bindable.Bind.Collections.oneWay source (nameof <@ d.Target @>) _nav t itemComponent

    //    let add = Bind.Explicit.createCommandParam (nameof <@ d.Add @>) source

    //    [
    //        add |> Observable.map Add
    //        updateItem |> Observable.map ChangeItem
    //    ]

    let bindToSource  =
        Component.create<Model, _, ModelMessage> [
            <@ d.Source @> |> Bind.oneWay (fun m -> m.Source)
            <@ d.Target @> |> Bind.collection (fun m -> m.Target) itemComponent ChangeItem
            <@ d.Add @>    |> Bind.cmdParam ModelMessage.Add
        ]

    let applicationCore = Framework.application Model.Default updateModel bindToSource (fun _ _ -> ()) // Needed to change nav here - with singleViewFromWindow, empty wasn't working

    [<STAThread>]
    [<EntryPoint>]
    let main _ =
        Framework.RunApplication (Navigation.singleViewFromWindow MainWindow, applicationCore)

        1
