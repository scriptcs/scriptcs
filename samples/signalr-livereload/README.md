# SignalR Live Reload #

In *your* project that uses SignalR, add the following hub:

    [HubName("liveReload")]
    public class LiveReloadHub : Hub
    {
        public void ReloadAllClients()
        {
            Clients.Others.reload();
        }
    }

Then, add the relevant markup to your Razor layout:

    <script src="/Scripts/jquery-1.6.4.min.js"></script>
    <script src="/Scripts/jquery.signalR-1.0.1.min.js"></script>
    <script src="/signalr/hubs"></script>
    <script type="text/javascript">
        $(function () {
            var hub = $.connection.liveReload;

            hub.client.reload = function () {
                document.location.reload(true);
            };

            $.connection.hub.start().done(function () {
                console.log("live reload ready");
            });
        });
    </script>

## Running the Live Reloader ##

1. Run `nuget install -o packages` to restore the packages listed in `packages.config`.
2. Run `scriptcs signalr.livereload.csx`
3. Edit `config.json` and fill in the blanks.
4. Run `scriptcs signalr.livereload.csx` again.
