var require = patchRequire(require);
//access the spreadsheet and do a for circle
var fs = require('fs');
//write a test file

exports.check_path = function (departure, destination, date)
{
        //build a browser
        var casper = require('casper').create({
            verbose: true,
            logLevel: "debug",
            clientScripts: ['jquery-3.2.1.min.js'],
            pageSettings: {
                loadImages: false,
                loadPlugins: true,
                // creat an agent
                userAgent: 'Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/27.0.1453.94 Safari/537.36'
            },
            viewportSize: {
                width: 1920,
                height: 1080
            }/*,
              encoding: 'utf8',
              headers: {
                        'Content-Type': 'application/json; charset=utf-8'
                      },
              method: 'get'*/
        });

        //build url for inquiry: date is current day, city should be obtained from a profile.
    casper.start('https://flight.qunar.com/site/oneway_list.htm?searchDepartureAirport=' + departure + '&searchArrivalAirport=' + destination +
                    '&searchDepartureTime=' + date);

        casper.waitFor(
            function check() {
                return this.evaluate(function () {
                    //once the page loads completely, check returns.
                    return document.querySelectorAll('p.airport').length > 0;

                });
            },
            function then() {
                //get flight info
                var airline = casper.evaluate(function lines() {
                    return document.querySelectorAll('div.b-airfly').length;
                });
                if (airline > 0) {
                    //click on DIRECT checkbox if there is one.
                    var direct = casper.evaluate(function direct() {
                        return $('div.item.item-direct input[type="checkbox"]').length;
                    });
                    if (direct > 0) {
                        casper.click('div.item.item-direct input[type="checkbox"]');
                    }
                    
                    var directAirline = casper.evaluate(function () {
                        var result;
                        //var stop_change = "";

                        //var trans = document.getElementsByClassName("b-airfly")[0].firstChild.firstChild.firstChild.lastChild.getElementsByClassName("trans");
                        var trans_lines = document.getElementsByClassName("b-airfly")[0].firstChild.firstChild.firstChild.getElementsByClassName("col-airline");
                        
                        //check if trans exists in the 1st airline in the page.
                        if (trans_lines.length > 0) {
                            var num_lines = trans_lines[0].getElementsByClassName("d-air").length;

                            if (num_lines > 1) {
                                result = "Transfer";
                            }
                            else {
                                if (num_lines == 1) {
                                    result = "Direct";
                                }
                            }
                            //20572 is the decimal value of Chinese charater: stop in Unicode; 36716 is for "transfer"
                            /*if (stop_change.charCodeAt(0) == 20572) {
                                result = "Yes";
                            }*/
                            
                        }

                        return result;
                    });
                    //no transferring
                    if (directAirline == "Direct") {
                        fs.write('check_path_interim' + '_' + departure + '_' + destination + '.txt', '1', 'w');
                    }
                    //"transit" in trans
                    if (directAirline == "Transfer") {
                        fs.write('check_path_interim' + '_' + departure + '_' + destination + '.txt', '0', 'w');
                    }
                    //test
                    //fs.write('check_path_interim' + '_' + departure + '_' + destination + '.txt', directAirline + ", " + new Date().getMinutes().toString() + ':' +
                      //  new Date().getSeconds().toString(), 'w');
                }
                //no airlines available
                else {
                    //write 0 into the file
                    fs.write('check_path_interim' + '_' + departure + '_' + destination + '.txt', "no airlines: " + airline, 'w');
                }

                //this.echo(directAirline);

            },
            function timeout() { //write 0 into the file
                fs.write('check_path_interim' + '_' + departure + '_' + destination + '.txt', "time out", 'w');
            }, 30000);

        casper.run();
}
