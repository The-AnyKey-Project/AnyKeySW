const Meta = imports.gi.Meta;
const Shell = imports.gi.Shell;

let focus_window_notify_connection = null;
let tracker = null;

function init()
{
	tracker = Shell.WindowTracker.get_default();
}

function enable()
{
	focus_window_notify_connection=global.display.connect('notify::focus-window',on_notify_focus_window);
	on_notify_focus_window();
}

function disable()
{
	global.display.disconnect(focus_window_notify_connection);
}

function on_notify_focus_window()
{
	let window=global.display.get_focus_window();

	if(!window)
		return;

	let app = tracker.get_window_app(window);
	let app_name = app.get_name();

	log("AnyKeyCompanion App Name: '" + app_name + "'");
	//log("AnyKeyCompanion get_app_info().get_filename(): " + app.get_app_info().get_filename()); // only GTK apps
	//log("AnyKeyCompanion get_app_info().get_generic_name(): " + app.get_app_info().get_generic_name()); // only GTK apps

	// TODO: do something with app_name
	
}
