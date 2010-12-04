
    //xml output
    function printXMLResult(queryResult) {

        if (queryResult != undefined) {

            var out = "";
            out += "<div class=\"resultItem\">";
            out += "<span class=\"AttrTag\">&lt;sones <span class=\"AttrParamName\">version</span>=<span class=\"AttrParamValue\">\"1.0\"</span>&gt;</span>\n";
            out += "<div class=\"inner\"><span class=\"AttrTag\">&lt;graphdb <span class=\"AttrParamName\">version</span>=<span class=\"AttrParamValue\">\"1.0\"</span>&gt;</span>\n";
            out += "<div class=\"inner\"><span class=\"AttrTag\">&lt;queryresult <span class=\"AttrParamName\">version</span>=<span class=\"AttrParamValue\">\"1.0\"</span>&gt;</span>\n";

            // query string
            var query = escapeArrowBrackets($(queryResult.getElementsByTagName("query")[0]).text());
            out += "<div class=\"inner\"><span class=\"AttrTag\">&lt;query&gt;</span>" + query + "<span class=\"AttrTag\">&lt;/query&gt;</span></div>\n";

            // query result
            var result = escapeArrowBrackets($(queryResult.getElementsByTagName("result")[0]).text());
            out += "<div class=\"inner\"><span class=\"AttrTag\">&lt;result&gt;</span>" + result + "<span class=\"AttrTag\">&lt;/result&gt;</span></div>\n";

            // duration
            var resolution = escapeArrowBrackets($(queryResult.getElementsByTagName("duration")[0]).attr("resolution"));
            var duration = escapeArrowBrackets($(queryResult.getElementsByTagName("duration")[0]).text());
            out += "<div class=\"inner\"><span class=\"AttrTag\">&lt;duration <span class=\"AttrParamName\">resolution</span>=<span class=\"AttrParamValue\">\"" + resolution + "\"</span>&gt;</span>" + duration + "<span class=\"AttrTag\">&lt;/duration&gt;</span></div>\n";

            // warnings
            out += ErrorsOrWarnings2XML(queryResult, "warnings");

            // errors
            out += ErrorsOrWarnings2XML(queryResult, "errors");

            // results        
            out += results2XML(queryResult.getElementsByTagName("results")[0], false);

            out += "<span class=\"AttrTag\">&lt;/queryresult&gt;</div></span>\n";
            out += "<span class=\"AttrTag\">&lt;/graphdb&gt;</div></span>\n";
            out += "<span class=\"AttrTag\">&lt;/sones&gt;</span>\n";
            out += "</div>\n";

            // unformated xml
            //out += "<pre>" + escapeArrowBrackets((new XMLSerializer()).serializeToString(queryResult)) + "</pre>";//.replace(/(\r\n|[\r\n])/g, "<br />");

            goosh.gui.out(out);

        }
    }

    // xml output
    function results2XML(resultSet, recursive) {

        var tmp = "";
        var results = $(resultSet).children();

        if (results.length == 0)
            tmp += "<span class=\"DBObjectTag\"><div class=\"inner\">&lt;" + resultSet.nodeName + " /&gt;</span></div>\n";

        else {

            if (!recursive)
                tmp += "<span class=\"DBObjectTag\"><div class=\"inner\">&lt;" + resultSet.nodeName + "&gt;</span>\n";

            for (var i = 0; i < results.length; i++) {

                // hyperedgelabel
                if (results[i].nodeName == "hyperedgelabel") {

                    if ($(results[i]).text() == "") {
                        tmp += "<div class=\"inner\"><span class=\"DBObjectTag\">&lt;hyperedgelabel /&gt;</span></div>";
                    }
                    else {
                        tmp += "<div class=\"inner\"><span class=\"DBObjectTag\">&lt;hyperedgelabel</span>";
                        tmp += $(results[i]).text();
                        tmp += "<span class=\"DBObjectTag\">&lt;/hyperedgelabel&gt;</span></div>";
                    }

                }

                // DBObject
                else if (results[i].nodeName == "vertex") {

                    tmp += "<div class=\"inner\"><span class=\"DBObjectTag\">&lt;vertex</span>";
                    tmp += "<span class=\"DBObjectTag\">&gt;</span>";

                    var attributeTags = $(results[i]).children();

                    for (var j = 0; j < attributeTags.length; j++) {

                        var attributeName = attributeTags[j].nodeName;

                        // edgelabel
                        if (attributeName == "edgelabel") {

                            if ($(attributeTags[j]).text() == "") {
                                tmp += "<div class=\"inner\"><span class=\"AttrTag\">&lt;edgelabel /&gt;</span></div>";
                            }
                            else {

                                tmp += "<div class=\"inner\"><span class=\"AttrTag\">&lt;edgelabel&gt;</span>";

                                var edgeLabels = $(attributeTags[j]).children();

                                for (var ii = 0; ii < edgeLabels.length; ii++) {
                                    tmp += "<div class=\"inner\"><span class=\"AttrTag\">&lt;" + edgeLabels[ii].nodeName + "</span>";
                                    for (var k = 0; k < edgeLabels[j].attributes.length; k++) {
                                        tmp += " <span class=\"AttrParamName\">" + edgeLabels[j].attributes[k].nodeName + "</span>=<span class=\"AttrParamValue\">\"" + edgeLabels[j].attributes[k].nodeValue + "\"</span>";
                                    }
                                    tmp += "<span class=\"AttrTag\">&gt;</span>" + $(edgeLabels[ii]).text();
                                    tmp += "<span class=\"AttrTag\">&lt;/" + edgeLabels[ii].nodeName + "&gt;</span></div>";
                                }

                                tmp += "<span class=\"AttrTag\">&lt;/edgelabel&gt;</span></div>";

                            }

                        }

                        // edge
                        else if (attributeName == "edge") {

                            // edge tag
                            tmp += "<div class=\"inner\"><span class=\"AttrTag\">&lt;" + attributeName + "</span>";
                            for (var k = 0; k < attributeTags[j].attributes.length; k++) {
                                tmp += " <span class=\"AttrParamName\">" + attributeTags[j].attributes[k].nodeName + "</span>=<span class=\"AttrParamValue\">\"" + attributeTags[j].attributes[k].nodeValue + "\"</span>";
                            }
                            tmp += "<span class=\"AttrTag\">&gt;</span>";

                            // RECURSION!
                            tmp += results2XML(attributeTags[j], true);

                            tmp += "<span class=\"AttrTag\">&lt;/" + attributeName + "&gt;</span></div>";

                        }

                        // attribute
                        else {

                            tmp += "<div class=\"inner\"><span class=\"AttrTag\">&lt;" + attributeName + "</span>";

                            for (var k = 0; k < attributeTags[j].attributes.length; k++) {
                                tmp += " <span class=\"AttrParamName\">" + attributeTags[j].attributes[k].nodeName + "</span>=<span class=\"AttrParamValue\">\"" + attributeTags[j].attributes[k].nodeValue + "\"</span>";
                            }

                            var items = $(attributeTags[j]).children();

                            if (items.length > 0) {

                                tmp += "<span class=\"AttrTag\">&gt;</span><span class=\"AttrTagValue\">"

                                for (var m = 0; m < items.length; m++) {
                                    tmp += "<div class=\"inner\"><span class=\"AttrTag\">&lt;" + items[m].nodeName + "&gt;</span>";
                                    tmp += $(items[m]).text()
                                    tmp += "<span class=\"AttrTag\">&lt;/" + items[m].nodeName + "&gt;</span></div>";
                                }

                                tmp += "</span><span class=\"AttrTag\">&lt;/" + attributeName + "&gt;</span></div>";

                            }

                            else
                                tmp += "<span class=\"AttrTag\">&gt;</span><span class=\"AttrTagValue\">" + escapeArrowBrackets($(attributeTags[j]).text()) + "</span><span class=\"AttrTag\">&lt;/" + attributeName + "&gt;</span></div>";

                        }

                    }

                    tmp += "<span class=\"DBObjectTag\">&lt;/" + results[i].nodeName + "&gt;</span></div>";

                }

            }

            if (!recursive)
                tmp += "<span class=\"DBObjectTag\">&lt;/" + resultSet.nodeName + "&gt;</span></div>\n";

        }

        return tmp;

    }


    // xml output
    function ErrorsOrWarnings2XML(source, typename) {

        var tmp = "";
        var AllErrorsOrWarnings = $(source.getElementsByTagName(typename)[0]).children();

        if (AllErrorsOrWarnings.length == 0)
            tmp += "<span class=\"DBObjectTag\"><div class=\"inner\">&lt;" + typename + " /&gt;</span></div>\n";

        else {

            tmp += "<span class=\"DBObjectTag\"><div class=\"inner\">&lt;" + typename + "&gt;</span>\n";

            for (var i = 0; i < AllErrorsOrWarnings.length; i++) {
                tmp += "<div class=\"inner\"><span class=\"bold\">&lt;" + AllErrorsOrWarnings[i].nodeName + " code=\"" + $(AllErrorsOrWarnings[i]).attr("code") + "\"&gt;</span>\n";
                tmp += "<div class=\"inner\">" + escapeArrowBrackets($(AllErrorsOrWarnings[i]).text()) + "</div>\n";
                tmp += "&lt;/" + AllErrorsOrWarnings[i].nodeName + "&gt;</div>\n";
            }

            tmp += "<span class=\"DBObjectTag\">&lt;/" + typename + "&gt;</span></div>\n";

        }

        return tmp;

    }

    //query handling
    function doQuery(args) {
        if (args.length > 0) {
            //build the query string
            var query = "";
            for (var i = 0; i < args.length; i++) {
                query += args[i] + " ";
            }
            //send the trimmed query
            return sendQuery(jQuery.trim(query));
        } else {
            return "";
        }
    }

    function escapeArrowBrackets(text) {
        text = text.replace(/</g, "&lt;");
        text = text.replace(/>/g, "&gt;");
        return text;
    }


    function sendQuery(query) {

        //replace apostrophe
        query = query.replace(/´/g, "'");
        query = query.replace(/\"/g, "'");
        //encode the query
        var encodedQuery = $.URLEncode(query);

        //build the target URI
        var target = goosh.config.webservice_protocol + "://"
               + goosh.config.webservice_host
               + ((goosh.config.webservice_port != undefined) ? (":" + goosh.config.webservice_port) : "")
               + goosh.config.webservice_path + "/"
               + goosh.config.mode + "?"
               + encodedQuery;

        //do some ajax
        var RESTResponse = $.ajax({
            url: target,
            cache: false,
            async: false,
            error: function (xhr, ajaxOptions, thrownError) {
                return ("AJAX Error " + xhr.status + "\n" + data.responseText + "\n" + thrownError);
            },
            beforeSend: function (xhr) {
                if (goosh.config.webservice_default_format == "xml")
                    xhr.setRequestHeader('Accept', 'application/xml');
                else if (goosh.config.webservice_default_format == "gexf")
                    xhr.setRequestHeader('Accept', 'application/gexf');
                else if (goosh.config.webservice_default_format == "text")
                    xhr.setRequestHeader('Accept', 'text/plain');
                else
                    xhr.setRequestHeader('Accept', 'application/json');
            }
        });

        if (RESTResponse == null)
            return "<span class=\"AttrTagValue\">Error: Empty result set!</span>";

        return handleRESTResult(RESTResponse);

    }

    function handleRESTResult(RESTResponse) {
        //strip the encoded result out of the result xml
        //the pattern strips the text between the string tag
        //var pattern = /(<string\b[^>]*>(.*?)<\/string>)/;
        //var result = pattern.exec(resultString);

        //result = $.base64Decode(resultString);
        result = RESTResponse.responseText;
        ContentType = RESTResponse.getResponseHeader('Content-Type');

        if (result != undefined && result != "") {

            // ServerSideException
            if (result.indexOf("ServerSideException") > -1)
                return "<span class=\"AttrTagValue\">Server Side Exception!</span>";

            // xml
            if (ContentType != null && ContentType.indexOf("application/xml") > -1) {

                // Parse XML from String       
                if (window.DOMParser) {
                    parser = new DOMParser();
                    xmlDoc = parser.parseFromString(result, "text/xml");
                }

                // Internet Explorer
                else {
                    xmlDoc = new ActiveXObject("Microsoft.XMLDOM");
                    var pi = xmlDoc.createProcessingInstruction("xml", " version='1.0' encoding='UTF-8'");
                    var XmlPattern = /(<?xml\sversion*)/;
                    var matchResult = XmlPattern.exec(result);

                    if (matchResult != undefined) {
                        var index = result.indexOf(">", 0);
                        if (index > 0)
                            result = result.slice(index + 1, result.length);
                    }

                    xmlDoc.appendChild(pi);
                    xmlDoc.async = "false";
                    xmlDoc.loadXML(result);
                }

                return xmlDoc;

            }

            // json
            else if (ContentType.indexOf("application/json") > -1)
                return '<pre class=\"AttrTagValue\">' + result + '</pre>';

            // text
            else if (ContentType.indexOf("text/plain") > -1)
                return '<pre class=\"AttrTagValue\">' + result + '</pre>';

            // error
            else
                return "<span class=\"AttrTagValue\">Error: Unknown content-type '" + ContentType + "'!</span>";

        }

        else
            return "<span class=\"AttrTagValue\">Communication error or error parsing queryresult from REST!</span>";

    }


    function UpdateDDate() {

        //build the target URI
        var target = goosh.config.webservice_protocol + "://"
               + goosh.config.webservice_host
               + ((goosh.config.webservice_port != undefined) ? (":" + goosh.config.webservice_port) : "")
               + "/"
               + goosh.config.webservice_path + "ddate";

        //do some ajax
        var html = $.ajax({
            url: target,
            cache: false,
            async: false,
            error: function () {
                return 1;
            }
        });

        if (html == null) {
            return 2;
        } else {
            goosh.gui.ddate.innerHTML = html.responseText;
            return 0;
        }

    };




    //extend Webshell
    $(document).ready(function () {
        //history module
        goosh.module.history = function () {

            this.name = "history";
            this.aliases = new Array("history");
            this.help = "show last commands";

            this.call = function (args) {
                var out = "";
                if (goosh.keyboard.hist.length > 0) {
                    out = "<ol class=\"historylist\">";
                    for (i = 0; i < goosh.keyboard.hist.length; i++) {
                        out += "<li>" + goosh.keyboard.hist[i] + "</li>";
                    }
                    out += "</ol>";
                }

                goosh.gui.outln(out);
            };
        };

        goosh.modules.register("history");
        //eo history module


        //the GUI is waiting for an AJAX-Response
        goosh.gui.waiting = false;
        goosh.gui.setWaiting = function (waitingFlag) {
            goosh.gui.waiting = (waitingFlag == true) ? true : false;
            if (goosh.gui.waiting) {
                $("<img src=\"/resources/WebShell/waitingimg.gif\" alt=\"processing...\" class=\"waitingimg\" />").appendTo("#prompt");
            } else {
                $(".waitingimg").remove();
            }
        };


        //update goosh-configuration
        goosh.config.user = "GraphDB";
        goosh.config.host = jQuery.url.attr("host");
        goosh.config.mode = "gql";
        goosh.config.webservice_protocol = jQuery.url.attr("protocol");
        goosh.config.webservice_host = jQuery.url.attr("host");
        goosh.config.webservice_path = jQuery.url.attr("directory").substring(0, jQuery.url.attr("directory").lastIndexOf('/'));
        goosh.config.webservice_port = jQuery.url.attr("port");
        goosh.config.webservice_default_format = "json";
        goosh.config.webservice_formats = new Array("xml", "json", "text", "gexf");

        //sones.licence
        goosh.module.license = function () {

            this.name = "license";
            this.aliases = new Array("license", "l");

            this.help = "displays license information";

            this.call = function (args) {

                var out = "";
                out += "<pre>";
                out += "Copyright (c) 2010, sones GmbH\n";
                out += "All rights reserved.\n";
                out += "\n";
                out += "New BSD License\n";
                out += "\n";
                out += "Redistribution and use in source and binary forms, with or without\n";
                out += "modification, are permitted provided that the following conditions are met:\n";
                out += "    * Redistributions of source code must retain the above copyright\n";
                out += "      notice, this list of conditions and the following disclaimer.\n";
                out += "    * Redistributions in binary form must reproduce the above copyright\n";
                out += "      notice, this list of conditions and the following disclaimer in the\n";
                out += "      documentation and/or other materials provided with the distribution.\n";
                out += "    * Neither the name of the sones GmbH nor the\n";
                out += "      names of its contributors may be used to endorse or promote products\n";
                out += "      derived from this software without specific prior written permission.\n";
                out += "\n";
                out += "THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS \"AS IS\" AND\n";
                out += "ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED\n";
                out += "WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE\n";
                out += "DISCLAIMED. IN NO EVENT SHALL sones GmbH BE LIABLE FOR ANY\n";
                out += "DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES\n";
                out += "(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;\n";
                out += "LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND\n";
                out += "ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT\n";
                out += "(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS\n";
                out += "SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.\n";
                out += "\n";
                out += "\n";
                out += "jQuery JavaScript Library - Copyright (c) 2009 John Resig\n";
                out += "Dual licensed under the MIT and GPL licenses.\n";
                out += "http://docs.jquery.com/License\n";
                out += "\n";
                out += "goosh is written by Stefan Grothkopp <grothkopp@gmail.com>\n";
                out += "goosh is open source under the Artistic License/GPL.\n";
                out += "http://www.goosh.org\n";
                out += "</pre>";
                goosh.gui.outln(out);
            }
        }

        goosh.modules.register("license");
        //eo sones.licence


        //GQL handler
        goosh.module.gql = function () {
            this.name = "gql";
            this.aliases = new Array("gql", "g");
            this.help = "switch to gql mode";

            this.call = function (args) {
                if (goosh.config.mode != "gql") {
                    goosh.config.mode = "gql";
                    goosh.gui.updateprompt();
                } else {
                    //result is XML
                    var result = doQuery(args);
                    if (result != undefined) {
                        if (goosh.config.webservice_default_format == 'xml') {
                            printXMLResult(result.firstChild);
                        } else if (goosh.config.webservice_default_format == 'gexf') {
                            printXMLResult(result.firstChild);
                        }
                        else if (goosh.config.webservice_default_format == 'json') { //json
                            /*
                            * json is currently displayed as one line string
                            * String can be parsed to JSON Object via eval()                    
                            */
                            //goosh.gui.out(printJSONResult(eval('(' + result + ')')));                    
                            goosh.gui.out(result);
                        } else { //text
                            goosh.gui.out(result);
                        }
                    } else { //result is undefined
                        goosh.gui.error("Internal");
                    }
                }
            }
        }
        goosh.modules.register("gql");
        //eo GQL handler
    });