(* this is the business logic for https://ipgs.okstate.edu/3A73CBA6-BC09-4A9E-AF59-FF1467EEFDDE/ and https://ipgs.okstate.edu/  *)

module ipgs
    
    open Prelude
    open ArcReaction

    open ISO.Data.IPGS

    open System.Net.Mail
    open MailTemplates
    open System.Text
    open System

    let adhoc_state s = { new AppState() with override x.GetRepresentation ctx = view s }

    let REDACTED = Unchecked.defaultof<string>

    module Verite =
        
        type SaveRegistration() =
            inherit AppState()

                override x.GetRepresentation ctx = 
                                                                                                                                                    
                        let result = read ctx {
                                                                                                                                                    
                                            let! fullname = get_str "fullname"
                                            let! dob = get_date "dob"
                                            let! sex = get_positive_int "sex"
                                            let! email =  get_str "email"
                                            let! citizenship = get_positive_int "citizenship"

                                            let! street1 = get_str "street1"
                                            let! street2 = maybe_str "street2"
                                            let! street3 = maybe_str "street3"
                                            let! postal =  get_str "postal"
                                            let! city =  get_str "city"
                                            let! province = maybe_str "non-us-territory"
                                            let! state = maybe_int "state"
                                            let! mailing_Country = get_positive_int "mailing_country"

                                            return !(Start_IPGS_CinemaVeriteParticipantApplication(
                                                                                                                                                                                    
                                                        fullname,
                                                        dob,
                                                        sex,
                                                        email,
                                                        (?) citizenship,
                                                        null,
                                                        street1,
                                                        street2,
                                                        street3,
                                                        postal,
                                                        city,
                                                        province,
                                                        state,
                                                        (?) mailing_Country,
                                                        null)) }

                        match result with
                        | Success r -> 

                            see_other("/verite/register/"+ r.ToString())

                        | Failed s -> view s

        let save_registration = new SaveRegistration() |> app        
        
        type RetreiveRegistrationForUpdate(id) =
            inherit AppState()
                override x.GetRepresentation ctx = view_model "/views/ipgs/verite_application.aspx" (!GetStartedCinemaVeiteRegistrationByID(id))
        
        type RetrieveSavedRegistration(id) =
            inherit AppState()

                override x.Accept msg =

                        let msg'= str msg

                        match msg' with
                        | "update" -> new RetreiveRegistrationForUpdate(id) |> app
                        | _ -> null
                                                                                                                                                                                
                                                                                                                                                                        
                override x.GetRepresentation ctx = 
                                                                                                                                                                            
                        view_model "/views/ipgs/review_application.aspx" (!GetStartedCinemaVeiteRegistrationByID(id))
                
        
        type Register() =
            inherit AppState()

                override x.Accept msg =
                                                                                                                                    
                            let msg' = str msg
                                                                                                                                    
                            match Guid.TryParse msg' with
                            | (true, id) ->  RetrieveSavedRegistration(id) |> app
                            | _ ->
                                                                                                                                    
                                                                                                                                    
                                match str msg with
                                | "make-it-so" -> save_registration
                                    

                                | _ -> null

                                                                                                                                    

                    override x.GetRepresentation ctx = view_model "/views/ipgs/verite_application.aspx" Unchecked.defaultof<ISO.Data.IPGS.CinemaVeriteRegistration>                                           


        let register = new Register() |> app
        let travel_grants = adhoc_state "/views/ipgs/special-travel-grant.aspx"
        let grants  = adhoc_state "/views/ipgs/travelgrants.aspx"
        let links   = adhoc_state  "/views/ipgs/verite-links.aspx"
        let visa    = adhoc_state "/views/ipgs/visa.aspx"

        type Root() = 
            inherit AppState()

                override x.Accept msg = 
                            match str msg with
                            | "visa" -> visa
                            | "links" -> links
                            | "grants" -> grants
                            | "travel-grants" -> travel_grants
                            | "register" ->  register                                                                               
                            | _ -> null     
                                                                         
                override x.GetRepresentation ctx = view "/views/ipgs/verite.aspx"
    
    
    module PersianLanguage =

        type Root() =
            inherit AppState()
                
                override x.Accept msg =
                    match str msg with
                    | "overview"        -> adhoc_state "/views/ipgs/root.aspx"
                    | "first-semester"  -> adhoc_state "/views/ipgs/intro-to-persian-gulf/1.aspx"
                    | "spring-break"    -> adhoc_state "/views/ipgs/intro-to-persian-gulf/springbreak.aspx"
                    | "summer"          -> adhoc_state "/views/ipgs/intro-to-persian-gulf/summer.aspx"
                    | "resources"       -> adhoc_state "/views/ipgs/intro-to-persian-gulf/resources.aspx"

                    | _ -> null
                override x.GetRepresentation ctx = null
    
    
    module Courses =
        type Root() =
            inherit AppState()
                override x.Accept msg =
                    match str msg with
                    | "intro-contemporary-iran-persian-gulf" -> PersianLanguage.Root() |> app
                    | _ -> null
                override x.GetRepresentation ctx = null
    
    
    module HandOnCameraConfirmaton =

        type Root() =
            inherit AppState()
                override x.Accept msg =
                    match System.Guid.TryParse(str msg) with
                    | (true, s) -> 
                        
                        let reg_info = !HandsOnCameraRegistrationByID(s)

                        if reg_info = null then null else { new AppState() with override x.GetRepresentation ctx = (* view_model "/views/ipgs/confirm-hands-on-camera.aspx" reg_info *) view "Thank you for registering." }

                    | _ -> null

                override x.GetRepresentation ctx = null

    module Subscribe =
        
        type [<NoComparison>] SuccessfulRegistration = {Name : string; FamilyName : string; Email :  string; Department : string; Keywords : string; Proc : Proc<int>  }

        type addr = MailAddress
        
        
        type RegisterForHandsOnCamera() =
            inherit AppState()
                override x.GetRepresentation ctx =
                    match read ctx {
                                        
                                        let! fullname = get_str "fullname"
                                        let! dob = get_date "dob"
                                        let! gender = get_int "gender"
                                        let! citizen = get_int "uscitizen"
                                        let! country_of_citizenship = if citizen = 0 then get_int "country" else Success 0
                                        let! oklahoma_resident = get_int "ok_resident"

                                        let! mailling_street = get_str "mailing_street"
                                        let! mailin_city = get_str "mailing_city"
                                        let! mailling_state = get_str "mailing_state"
                                        let! mailling_zip = get_str "mailing_zip"
                                        let! mailling_country = get_str "mailing_country"
                                        
                                        let! telephone = get_str "telephone"
                                        let! email = get_str "email"

                                        (*

                                        let! card_type = get_int "cardtype"
                                        let! cardholder = get_str "cardholder"
                                        let! cardnumber = get_str "cardnumber"
                                        let! exp_date = get_str "exp_date"
                                        let! security_code = get_str "card_security" *)
                                        
                                        let! billing_street = get_str "billing_street"
                                        let! billing_city = get_str "billing_city"
                                        let! billing_state = get_str "billing_state"
                                        let! billing_zip = get_str "billing_zip"
                                        let! billing_country = get_str "billing_country"
                                        let! billing_telephone = get_str "billing_telephone"

                                        let! applicant_sig = get_str "applicant_signature"

                                        return new CreateHandsOnCameraRegistration
                                                        (
                                                            fullname,
                                                            dob,
                                                            gender,
                                                            country_of_citizenship,
                                                            oklahoma_resident,

                                                            mailling_street,
                                                            mailin_city,
                                                            mailling_state,
                                                            mailling_zip,
                                                            mailling_country,

                                                            telephone,
                                                            email,

                                                            0,
                                                            null,
                                                            null,
                                                            null,
                                                            null,

                                                            billing_street,
                                                            billing_city,
                                                            billing_state,
                                                            billing_zip,
                                                            billing_country,
                                                            billing_telephone,

                                                            applicant_sig) }
                            with
                            
                            | Success s -> 
                                            let id = !s

                                            let fullname = ctx.Request.Form.["fullname"]

                                            let fn o =
                                                let client = new SmtpClient("smtp.gmail.com", 587) //new SmtpClient("ieo-fs02.ue.okstate.edu")

                                                client.EnableSsl <- true                                                
                                                
                                                let msg = new MailMessage(addr("news@ipgs.okstate.edu", "Iranian and Persian Gulf Studies"), addr("pedram.khosronejad@okstate.edu", "Pedram Khosronejad"))

                                                
                                                msg.CC.Add(MailAddress("rodrick.chapman@okstate.edu", "Rodrick Chapman"))
                                                

                                                let creds = System.Net.NetworkCredential("intl.education.and.outreach@gmail.com", REDACTED)
                                                
                                                let email = new MailTemplates.IPGS.HandOnCameraApplicationPrintedNotice(fullname, "https://ipgs.okstate.edu/hands-on-camera/applications/" + id.ToString())


                                                msg.Subject <- "Notice of Hands on Camera Form"
                                                
                                                
                                                msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(email.GetHTML(), System.Net.Mime.ContentType("text/html")))
                                                msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(email.GetPlainText(), System.Net.Mime.ContentType("text/plain")))
                                                
                                                
                                                //msg.Body <- email.GetHTML()
                                                msg.IsBodyHtml <- true

                                                client.Credentials <- creds
                                                client.Send(msg)
                                                    
                                            let work = new QueuedWorkItem(System.Threading.WaitCallback(fn))

                                            view "Thank you for registering."

                                            //see_other("/" + id.ToString())

                            | Failed s -> view ("Failed " +  s)

                    

        type MakeItSo() =
            inherit AppState()
                override x.GetRepresentation ctx =
                    match read ctx {
                                        let! fullname = get_str "fullname"
                                        let! familyname =  get_str "family_name"
                                        let! email = get_str "email"
                                        let! dept = get_str "department"
                                        let! keywords = maybe_str "keywords"

                                        return { 
                                                    Name = fullname; 
                                                    FamilyName = familyname; 
                                                    Email = email; 
                                                    Department = dept; 
                                                    Keywords = keywords; 
                                                    Proc =  MakeIPGSSubscription(fullname, familyname, email, dept,  keywords) } }

                        with
                        | Success x -> !(x.Proc) |> ignore 
                                            
                                       let fn o =
                                                let client = new SmtpClient("smtp.gmail.com", 587) //new SmtpClient("ieo-fs02.ue.okstate.edu")

                                                client.EnableSsl <- true                                                
                                                
                                                let msg = new MailMessage(addr("news@ipgs.okstate.edu", "Iranian and Persian Gulf Studies"), addr("pedram.khosronejad@okstate.edu", "Pedram Khosronejad"))
                                                
                                                msg.CC.Add(MailAddress("rodrick.chapman@okstate.edu", "Rodrick Chapman"))                                                

                                                let creds = System.Net.NetworkCredential("intl.education.and.outreach@gmail.com", REDACTED)
                                                
                                                let email = new MailTemplates.IPGS.SubscriptionNotice(x.Email, x.Name, x.Department, x.Keywords)

                                                msg.Subject <- "Notice of IPGS Registration"                                                
                                                
                                                msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(email.GetPlainText(), System.Net.Mime.ContentType("text/plain")))
                                                msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(email.GetHTML(), System.Net.Mime.ContentType("text/html")))
                                                
                                                //msg.Body <- email.GetHTML()
                                                msg.IsBodyHtml <- true


                                                client.Credentials <- creds
                                                client.Send(msg)
                                                    
                                       let work = new QueuedWorkItem(System.Threading.WaitCallback(fn))
                        
                                       found("/registration-complete").GetRepresentation ctx

                        | Failed s -> null

    
        type View() =
            inherit AppState()
                override x.GetRepresentation ctx = view_model "/views/ipgs/viewsubs.aspx" (!GetIPGSSubscription())
        
        
        type Root() =
            inherit AppState()
                override x.Accept msg = if str msg = "make_it_so" then MakeItSo() |> app else if str msg = "view" then View() |>app else null
                override x.GetRepresentation ctx = view "/views/ipgs/subscribe.aspx"
    
    
    module BecomingHome =
    
        let kv k v xs = [System.Collections.Generic.KeyValuePair<string, string>(k, v)] @ xs
        
        [<AbstractClass>]
        type AppState(crumbs : System.Collections.Generic.KeyValuePair<string, string> list) =
                inherit ArcReaction.AppState()

        module People =            
            
            type AssociatedFaculty(crumbs) =
                inherit AppState(crumbs)
                    override x.GetRepresentation ctx = view "/views/ipgs/devsite/people/assoc-faculty.aspx"


            type ExecCommittee(crumbs) =
                inherit AppState(crumbs)
                    override x.GetRepresentation ctx = view "/views/ipgs/devsite/people/exec.aspx"

            type Staff(crumbs) =
                inherit AppState(crumbs)
                    override x.GetRepresentation ctx = view "/views/ipgs/devsite/people/staff.aspx"

            type AssociateDirector(crumbs) =
                inherit AppState(crumbs)
                    override x.GetRepresentation ctx = view "/views/ipgs/devsite/people/director.aspx"
    
            type Root(crumbs) =
                inherit AppState(crumbs)
                    
                    override x.Accept msg =
                        
                        let s' = str msg
                        
                        match s' with
                        | "associated-faculty" -> AssociatedFaculty(kv s' "" crumbs) |> app
                        | "exec-committee" -> ExecCommittee(kv s' "" crumbs) |> app
                        | "staff" -> Staff(kv s' "" crumbs) |> app
                        | "associate-director" -> AssociateDirector(kv s' "" crumbs) |> app
                        
                        | _ -> null
                    
                    override x.GetRepresentation msg = view "/views/ipgs/devsite/people.aspx"
        
        
        type About(crumbs) =
            inherit AppState(crumbs)
                override x.GetRepresentation msg = view "/views/ipgs/devsite/about.aspx"                

        type Resources(crumbs) =
            inherit AppState(crumbs)
                override x.GetRepresentation msg = view "/views/ipgs/devsite/resources.aspx"                

        type Giving(crumbs) =
            inherit AppState(crumbs)
                override x.GetRepresentation msg = view "/views/ipgs/devsite/giving.aspx"                

        module Events =  
        
           type DoAddEvent(title, date, category, file_name, file_data) =
                inherit ArcReaction.AppState()

                override x.GetRepresentation ctx = ISO.Data.Web.AddIPGSEvent(title, date, category, file_name, file_data) :> Representation

            type AddEvent(crumbs) =
                inherit AppState(crumbs)

                override x.Accept msg =
                    match str msg with
                    | "make_it_so" ->

                        let result = 
                            read (msg.Context) {
                                                    let! title = get_str "event_title"
                                                    let! date =  get_date "event_date"
                                                    let! category = get_int "event_category"
                                                    
                                                    let! file = 
                                                        
                                                        let file = msg.Context.Request.Files.["fileinput"]
                                                        
                                                        if file <> null
                                                        then
                                                            
                                                            let file = msg.Context.Request.Files.[0]
                                                            
                                                            Success (file.FileName, file.InputStream)
                                                        else
                                                            Failed "Expected a file."

                                                    
                                                    let (file_name, file_data) = file
                                                    


                                                    return (title, date, category, file_name, file_data) }

                        match result with
                        | Success (title, date, category, file_name, file_data) -> DoAddEvent(title, date, category, file_name, file_data) :> ArcReaction.AppState
                        | Failed s -> { new ArcReaction.AppState() 
                                            with override x.GetRepresentation ctx =
                                                    
                                                    ArcReaction.AdHocRepresentation((fun c ->                                                                                                    
                                                                                            c.Response.StatusCode <- 404
                                                                                            c.Response.Write(s) )) :> Representation
                                                    
                                                     }
                         

                    | _ -> null

                override x.GetRepresentation ctx = view "/views/ipgs/devsite/addevent.aspx"
           
           
           type IPGS(crumbs) =
                inherit AppState(crumbs)

                override x.GetRepresentation ctx = view "IPGS speakers..."

           type Fazel(crumbs) =
                inherit AppState(crumbs)

                override x.GetRepresentation ctx = 
                        let result = !ISO.Data.IPGS.GetIPGSEvents()

                        view "success"

                        //view_model "/views/ipgs/devsite/events.aspx" ("fazel", seq { for c in crumbs -> c}, seq { for r in result.Events -> r  }, 100)
                    

           type Farzaneh(crumbs) =
             inherit AppState(crumbs)

                override x.GetRepresentation ctx = view "Farzaneh..."
                       
                        


           type Speaker(crumbs) =
                inherit AppState(crumbs)

                    override x.Accept msg =
                        match str msg with
                        | "ipgs" -> IPGS(crumbs) :> ArcReaction.AppState
                        | "fazel" -> Fazel(crumbs) :> ArcReaction.AppState
                        | "farzaneh" -> Farzaneh(crumbs) :> ArcReaction.AppState
                        | _  -> null

                    override x.GetRepresentation ctx = 
                    
                                let result = !GetIPGSEvents()
                                
                                view_model "/views/ipgs/devsite/events.aspx" result

                                //view_model "/views/ipgs/devsite/events.aspx" ("speakers", seq { for c in crumbs -> c}, seq { for r in result.Events -> r }, 100)
            
            type Root(crumbs) =
                inherit AppState(crumbs)
                
                    member x.MapCategory msg' =

                        match msg' with
                                | "tea" | "fazel" | "ipgs" | "farzaneh" -> 


                                    if msg' = "speakers" 
                                    then Speaker(crumbs) :> ArcReaction.AppState
                                    else
                                    { new AppState(crumbs) 
                                            
                                                        with 
                                            
                                                            override x.Accept msg =
                                                    
                                                                match Int32.TryParse(str msg) with
                                                                | (true, n) ->

                                                                    let n = Nullable<int>(n)
                                                    
                                                                    { new AppState(crumbs) with                                                        
                                                        
                                                                        override x.GetRepresentation ctx = 

                                                                                                    let result = !GetIPGSEvents(n)

                                                                                                    view_model "/views/ipgs/devsite/events.aspx" (result, n, msg')
                                                    
                                                                    } :> ArcReaction.AppState

                                                                | _ -> null
                                                
                                                
                                                            override x.GetRepresentation ctx = view_model "/views/ipgs/devsite/events.aspx" (!GetIPGSEvents(), Nullable<int>(), msg') } :> ArcReaction.AppState

                                | "add" -> AddEvent(crumbs) :> ArcReaction.AppState
                        
                                | _ -> null

                        
                    
                    override x.Accept msg = 
                    
                        let msg' = str msg

                        match msg' with
                        | Int n -> 
                            let result = match n with 
                                         | 1 -> "tea"
                                         | 2 -> "fazel"
                                         | 3 -> "farzaneh"
                                         | 4 -> "ipgs"
                                         | _ ->  null

                            if result <> null then x.MapCategory(result) else null



                        | _ -> x.MapCategory msg'
                
                override x.GetRepresentation ctx = 
                
                    let r = !GetIPGSEvents()

                    let s = r.NearestSemester.ID
                        

                    found("/3A73CBA6-BC09-4A9E-AF59-FF1467EEFDDE/events/tea/" + (s.ToString())  ).GetRepresentation ctx

        type Outreach(crumbs) =
            inherit AppState(crumbs)
                override x.GetRepresentation msg = view "/views/ipgs/devsite/outreach.aspx"                

        type Links(crumbs) =
            inherit AppState(crumbs)
                override x.GetRepresentation msg = view "/views/ipgs/devsite/links.aspx"                

        type Media(crumbs) =
            inherit AppState(crumbs)
                override x.GetRepresentation msg = view "/views/ipgs/devsite/media.aspx"                

        type Contact(crumbs) =
            inherit AppState(crumbs)
                override x.GetRepresentation msg = view "/views/ipgs/devsite/contact.aspx"                

        type Courses(crumbs) =
            inherit AppState(crumbs)
                
                override x.Accept msg =
                    match System.Int32.TryParse(str msg) with
                    | (true, n) ->
                        
                        { new AppState(kv (n.ToString()) "" crumbs ) 
                            with override x.GetRepresentation ctx = view_model "/views/ipgs/devsite/courses.aspx" (System.Nullable<int>(n), seq { for c in crumbs -> c }) } :> ArcReaction.AppState

                    | _ -> null
                         
                
                override x.GetRepresentation msg = view "/views/ipgs/devsite/courses.aspx"

        type Root() =
            inherit ArcReaction.AppState()

                override x.Accept msg =
                    
                    let kv s = [System.Collections.Generic.KeyValuePair<string, string>(str msg, s)]
                    
                    match str msg with

                    | "about"       -> About(kv "") |> app
                    | "resources"   -> Resources(kv "") |> app
                    | "giving"      -> Giving(kv "") |> app
                    | "events"      -> Events.Root(kv "") |> app
                    | "outreach"    -> Outreach(kv "") |> app
                    | "links"       -> Links(kv "") |> app
                    | "media"       -> Media(kv "") |> app
                    | "contact"     -> Contact(kv "") |> app
                    | "people"      -> People.Root(kv "") |> app
                    | "courses"     -> Courses(kv "") |> app
                    | _ -> null

                override x.GetRepresentation ctx = view "/views/ipgs/devsite/home.aspx"
    
    
    type Root() =
        inherit AppState()
            override x.Accept msg = 
                        match str msg with
                        
                        | "seminar" | "seminars" | "seminar(s)" -> { new AppState() with 
                                                                            
                                                                            override x.Accept msg =
                                                                                match str msg with
                                                                                | "all" -> { new AppState() with override x.GetRepresentation ctx = view "/views/ipgs/seminar-detail.aspx" }
                                                                                | _ -> null
                                                                            
                                                                            override x.GetRepresentation ctx = view "/views/ipgs/seminar.aspx" }
                        
                        | "fazel-speakers" -> { new AppState() with 
                                                    
                                                    override x.Accept msg =
                                                        match str msg with
                                                        | "milani" -> { new AppState() with override x.GetRepresentation ctx = view "/views/ipgs/milani.aspx" }
                                                        | "karimi" -> { new AppState() with override x.GetRepresentation ctx = view "/views/ipgs/karimi.aspx" }
                                                        | _ -> null

                                                    
                                                    override x.GetRepresentation ctx = view "/views/ipgs/fazel.aspx" }
                        
                        | "subscribe" -> Subscribe.Root() |> app
                        | "hands-on-camera" -> { new AppState() 
                                                    with 
                                                        override x.GetRepresentation ctx = view "/views/ipgs/hands-on-camera.aspx"
                                                        override x.Accept msg' = 
                                                                    match str msg' with
                                                                    | "register" -> { new AppState()
                                                                                        with
                                                                                            override x.GetRepresentation ctx = view "/views/ipgs/hands-on-camera-registration.aspx" }

                                                                    | "accept" ->  Subscribe.RegisterForHandsOnCamera() |> app

                                                                    | "confirm" -> HandOnCameraConfirmaton.Root() |> app

                                                                    | "application" 
                                                                    | "applications" -> { new AppState()
                                                                                            with 
                                                                                                override x.Accept msg =
                                                                                                    match System.Guid.TryParse(str msg) with
                                                                                                    | (true, n) -> 
                                                                                                        let application = !HandsOnCameraRegistrationByID(n)

                                                                                                        if application = null then null 
                                                                                                                                else { new AppState() with
                                                                                                                                          override x.GetRepresentation ctx = 
                                                                                                                                            view_model "/views/ipgs/confirm-hands-on-camera.aspx" application }

                                                                                                    | _ -> null

                                                                                                override x.GetRepresentation ctx = null }

                                                                    | _ -> null }
                                                                        

                        | "3A73CBA6-BC09-4A9E-AF59-FF1467EEFDDE" -> BecomingHome.Root() |> app

                        | "registration-complete" -> { new AppState() with override x.GetRepresentation ctx = view "Registration successful" }
                        | "test" -> { new AppState() with override x.GetRepresentation ctx = view "/views/ipgs/test.aspx" }
                        | "courses" -> Courses.Root() |> app
                        | "persian-gulf-course" -> { new AppState() with override x.GetRepresentation ctx = see_other("/courses/intro-contemporary-iran-persian-gulf/overview") }
                        | "links" -> { new AppState() with override x.GetRepresentation ctx = view "/views/ipgs/links.aspx" }
                        | "cinema- verite" -> { new AppState() with override x.GetRepresentation ctx = see_other("/cinema-verite") }
                        | "verite" | "~verite~" | "cinema-verite" -> Verite.Root() |> app 

                        | "oxford" -> adhoc_state "/views/ipgs/intro-to-persian-gulf/summer.aspx"
                        | "ann-arbor" | "michigan" | "ann-arbor,michgan" -> adhoc_state "/views/ipgs/intro-to-persian-gulf/springbreak.aspx"

                        | "post_back" -> { new AppState() with  
                        
                                                override x.GetRepresentation ctx =
                                                    
                                                    let app_id = ctx.Request.Form.["EXT_TRANS_ID"] |> Guid.Parse
                                                    let payment = ctx.Request.Form.["pmt_amt"] |> Decimal.Parse
                                                    let payment_date = ctx.Request.Form.["pmt_date"] |> DateTime.Parse
                                                    let trans_id = ctx.Request.Form.["tpg_trans_id"]
                                                    let tracking_id = ctx.Request.Form.["sys_tracking_id"]
                                                    
                                                    let result = !ISO.Data.CompleteCinemaVeriteRegistration(app_id, payment, payment_date, trans_id, tracking_id)
                                                    

                                                    view "success" }

                        | "success" ->  { new AppState() with override x.GetRepresentation ctx = view "Success" }
                        | "failed" ->   { new AppState() with override x.GetRepresentation ctx = view "Failed" }

                        | "$$$" ->
                                    { new AppState() with

                                        override x.Accept msg =
                                            match Guid.TryParse(str msg) with
                                            | (true, id) -> { new AppState() with override x.GetRepresentation ctx = ISO.Data.CachedImageRepresentation(id) :> Representation }
                                            | _ -> null
                            
                                        override x.GetRepresentation ctx = null }


                        | "bg" -> 
                                
                                let t = msg.Context.Request.QueryString.["t"]

                                if System.String.IsNullOrWhiteSpace(t) |> not
                                then    
                                    
                                    let t' = t |> Convert.FromBase64String |> System.Text.Encoding.Unicode.GetString 

                                    { new AppState() with override x.GetRepresentation ctx = new Arc.VB.BackgroundText(t' , "dsfd", 16uy, 2000) :> Representation }
                                
                                else null
                        
                        
                        | _ -> null

            override x.GetRepresentation ctx = SeeOther("/subscribe").GetRepresentation ctx
