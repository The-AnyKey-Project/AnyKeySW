const Meta=imports.gi.Meta;
const Shell=imports.gi.Shell;

let focus_window_notify_connection=null;
let window_title_notify_connection=null;
let window=null;
let tracker = null;

function switch_layer()
{
	if(!window)
		return;
	let title = window.get_title();
	let app = tracker.get_window_app(window);
	log("gtk app id: " + window.get_gtk_application_id());
	log(app.get_name());
	log(app.get_app_info().get_filename());
}

function on_focus_window_notify()
{
	if(window && window_title_notify_connection)
		window.disconnect(window_title_notify_connection);

	window=global.display.get_focus_window();

	if(!window)
		return;

	let app = tracker.get_window_app(window);

	log("AnyKeyCompanion get_appname(): " + app.get_name());
	log("AnyKeyCompanion get_app_info().get_filename(): " + app.get_app_info().get_filename());

//	window_title_notify_connection=window.connect("notify::title",switch_layer);
//	switch_layer();
}


function enable()
{
	focus_window_notify_connection=global.display.connect('notify::focus-window',on_focus_window_notify);
	on_focus_window_notify();
}

function disable()
{
	global.display.disconnect(focus_window_notify_connection);

	if(window && window_title_notify_connection)
		window.disconnect(window_title_notify_connection);

	AppMenu._sync();
}

function init()
{
	tracker = Shell.WindowTracker.get_default();
}
