
function ProgressNotifier() {
    // private fields
    var self = this;
    this.notifier = null;
    this.percent = 0;
    this.serverProgressPercent = 0;
    this.progressInterval = null;

    this.updateProgress = function (percent) {
        self.serverProgressPercent = parseInt(percent);
    }

    this.startProgress = function () {
        self.notifier = new PNotify({
            title: "Processing Ending Reservations...",
            text: self.percent + "% complete.",
            addclass: 'custom',
            type: 'info',
            icon: 'fa fa-spinner fa-spin',
            hide: false,
            buttons: {
                closer: false,
                sticker: false
            },
            opacity: .75,
            shadow: false,
            width: "170px"
        });

        self.progressInterval = window.setInterval(function () {
            if (self.percent <= self.serverProgressPercent) {
                //console.log(self.percent);
                var options = {
                    //title: false,
                    text: self.percent + "% complete."
                };

                if (self.percent >= 70) {
                    options.title = "Almost There";
                }

                if (self.percent >= 100) {
                    window.clearInterval(self.progressInterval);
                    options.title = "Done!";
                    options.text = "Reservations updated sucessfully.";
                    options.type = "success";
                    options.icon = PNotify.prototype.options.icon;
                    options.hide = true;
                    options.buttons = {
                        closer: true,
                        sticker: true
                    };
                    options.opacity = 1;
                    options.shadow = true;
                    options.width = PNotify.prototype.options.width;
                    options.delay = 2000;
                }

                self.notifier.update(options);
                self.percent += 1;
            }
        }, 1);
    }

    return {
        Start: function () {
            self.startProgress();
        },
        Update: function (percent) {
            self.updateProgress(percent);
        }
    };
}
