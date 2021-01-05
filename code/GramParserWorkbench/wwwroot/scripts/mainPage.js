function startScript(parsingApiUrl, environment) {
    var grammar = document.getElementById('grammarText');
    var sample = document.getElementById('sampleText');
    var analysis = document.getElementById('analysisText');
    var body = document.getElementsByTagName("BODY")[0];
    var envAttribute = document.createAttribute("env");

    //  Setup environment
    envAttribute.value = environment;
    body.setAttributeNode(envAttribute);
    //  Setup hooks
    grammar.oninput = function () {
        onAnalysisInputChanged(grammar, sample, analysis, parsingApiUrl);
    };
    sample.oninput = function () {
        onAnalysisInputChanged(grammar, sample, analysis, parsingApiUrl);
    };
    readCookies(grammar, sample);
}

var isAnalyzing = false;
var isAnalysisQueued = false;
var lastAnalysisTime = 0;

function setCookies(grammarText, sampleText) {
    var d = new Date();

    d.setTime(d.getTime() + (24 * 60 * 60 * 1000 * 365 * 20));

    var expires = "expires=" + d.toUTCString();

    document.cookie = "grammar=" + escape(grammarText) + ";" + expires;
    document.cookie = "sample=" + escape(sampleText) + ";" + expires;
}

function readCookies(grammar, sample) {
    var memory = {};

    if (document.cookie.trim().length > 0) {
        var parts = document.cookie.split(';');

        for (var i = 0; i !== parts.length; ++i) {
            var subParts = parts[i].split('=');

            memory[subParts[0].trim()] = unescape(subParts[1].trim());
        }
    }
    if (memory.hasOwnProperty('grammar') && typeof memory.grammar === "string") {
        grammar.value = memory.grammar;
    }
    else {
        grammar.value = '#  Sample:\nrule main = ("a".."z" | "A".."Z")*;';
    }
    if (memory.hasOwnProperty('sample') && typeof memory.sample === "string") {
        sample.value = memory.sample;
    }
    else {
        sample.value = 'abc';
    }

    grammar.oninput();
}

function onAnalysisInputChanged(grammar, sample, analysisText, parsingApiUrl) {
    setCookies(grammar.value, sample.value);
    if (isAnalyzing) {
        isAnalysisQueued = true;
    }
    else {
        queueAnalysis(grammar, sample, analysisText, parsingApiUrl);
    }
}

function queueAnalysis(grammar, sample, analysisText, parsingApiUrl) {
    const DEFAULT_DELAY = 2000;

    var currentTime = new Date().getTime();
    var delta = currentTime - lastAnalysisTime;

    isAnalyzing = true;
    if (delta < DEFAULT_DELAY) {
        //  Wait a little before firing again to avoid drilling the service
        setTimeout(
            function () {
                requestAnalysis(grammar, sample, analysisText, parsingApiUrl);
            },
            delta);
    }
    else {
        //  No need to wait, request immediately
        requestAnalysis(grammar, sample, analysisText, parsingApiUrl);
    }
}

function requestAnalysis(grammar, sample, analysisText, parsingApiUrl) {
    var request = new XMLHttpRequest();
    var inputPayload = { "grammar": grammar.value, "text": sample.value };

    analysisText.classList.add("callInProgress");
    isAnalysisQueued = false;
    request.onreadystatechange = function () {
        if (this.readyState == 4) {
            lastAnalysisTime = new Date().getTime();

            if (this.status >= 200 && this.status < 300) {
                analysisText.value = this.status
                    + ":  "
                    + JSON.stringify(JSON.parse(this.responseText), null, 2);
                analysisText.classList.add("successAnalysis");
                analysisText.classList.remove("failureAnalysis");
            }
            else {
                analysisText.value = this.status + ":  " + this.responseText;
                analysisText.classList.add("failureAnalysis");
                analysisText.classList.remove("successAnalysis");
            }
            if (isAnalysisQueued) {
                queueAnalysis(grammar, sample, analysisText);
            }
            else {
                isAnalyzing = false;
                analysisText.classList.remove("callInProgress");
            }
        }
    };
    request.open("post", parsingApiUrl, true);
    request.setRequestHeader("Content-Type", "application/json");
    request.setRequestHeader("Accept", "application/json");
    request.send(JSON.stringify(inputPayload));
}