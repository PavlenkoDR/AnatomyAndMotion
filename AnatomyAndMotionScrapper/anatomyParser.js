function downloadBase64File(contentBase64, fileName) {
    const linkSource = `${contentBase64}`;
    const downloadLink = document.createElement('a');
    document.body.appendChild(downloadLink);

    downloadLink.href = linkSource;
    downloadLink.target = '_self';
    downloadLink.download = fileName;
    downloadLink.click();
    downloadLink.remove();
}

function downloadVideo(url, fileName) {
    fetch(url, {
        method: 'GET',
        mode: 'cors',
        credentials: 'include',
        headers: {
            'Access-Control-Allow-Origin': '*',
            'Access-Control-Allow-Methods': 'GET, POST, PUT, DELETE, OPTIONS'
        }
    }).then(res => res.blob()).then(file => {
        var tempUrl = URL.createObjectURL(file);
        downloadBase64File(tempUrl, fileName);
        URL.revokeObjectURL(tempUrl);
    });
}

function delay(time) {
    return new Promise(resolve => setTimeout(resolve, time));
}

async function downloadContent(fileNameBody) {
    var videos = document.getElementsByTagName("video");
    for (var i = 0; i < videos.length; i++) {
        downloadVideo(videos[i].src, `${fileNameBody}.mp4`);
        await delay(200);
    }
    var canvases = document.getElementsByTagName("canvas");
    for (var i = 0; i < canvases.length; i++) {
        downloadBase64File(canvases[i].toDataURL("image/png"), `${fileNameBody}.png`);
        await delay(200);
    }
}

function downloadText(filename, text) {
    var element = document.createElement('a');
    element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text));
    element.setAttribute('download', filename);

    element.style.display = 'none';
    document.body.appendChild(element);

    element.click();

    document.body.removeChild(element);
}

/////////////////////////////////////////////////////////////

function translate(language, word) {
    switch(word) {
        case "Videos:": return "Видео:";
        case "Origin": return "Начало";
        case "More": return "БОЛЬШЕ";
        case "Strength": return "Сила";
        case "Stretch": return "Растяжение";
    }
    return "";
}

function getMuiFormControls(node) {
    var nodes = [];
    try {
        if (node.className.includes('MuiFormGroup')) {
            nodes.push(node);
        }
        else {
            for (var i = 0; i < node.children.length; i++) {
                var result = getMuiFormControls(node.children[i]);
                if (result.length > 0) {
                    nodes = nodes.concat(result);
                }
            }
        }
    }
    catch(e) {
    }
    
    return nodes;
}

function getInputs(node) {
    var nodes = node.getElementsByTagName("input");
    for (var i = 0; i < node.children.length; i++) {
        var result = getMuiFormControls(node.children[i]);
        if (result.length > 0) {
            nodes = nodes.concat(result);
        }
    }
    return nodes;
}

function getMuiPaperRoots(node) {
    var nodes = [];
    try {
        if (node.className.includes('MuiPaper-root')) {
            nodes.push(node);
        }
        else {
            for (var i = 0; i < node.children.length; i++) {
                var result = getMuiPaperRoots(node.children[i]);
                nodes = nodes.concat(result);
            }
        }
    }
    catch(e) {
    }
    return nodes;
}

function getMuiPaperRootWithVideos(node) {
    try {
        if (node.className.includes('MuiPaper-root') && node.innerHTML.includes(translate('ru', 'Videos:'))) {
            return node
        }
        else {
            for (var i = 0; i < node.children.length; i++) {
                var result = getMuiPaperRootWithVideos(node.children[i]);
                if (result != null) {
                    return result;
                }
            }
        }
    }
    catch(e) {
    }
    return null;
}

function getDivWithVideoButtons(node) {
    try {
        if (!node.innerHTML.includes(translate('ru', 'Videos:'))) {
            return null;
        }
        else {
            var divs = node.getElementsByTagName('div');
            for (var i = 0; i < divs.length; i++) {
                var result = getDivWithVideoButtons(divs[i]);
                if (result != null) {
                    return result;
                }
            }
            if (node.innerHTML.includes(translate('ru', 'Videos:'))) {
                return node;
            }
        }
    }
    catch(e) {
    }
    return null;
}

function getButtonsFromDiv(node) {
    return node.getElementsByTagName('button');
}

function getImageDescription(node) {
    var imageDescription = "";
    if (node.children.length == 0) {
        var linesSrc = node.innerHTML;
        imageDescription += linesSrc + '\n';
    }
    else {
        var lines = node.children;
        for (var i = 0; i < lines.length; ++i) {
            imageDescription += getImageDescription(node.children[i]);
        }
    }
    return imageDescription;
}

function clickOriginInsertion(node) {
    var buttons = node.getElementsByTagName('button');
    for (var i = 0; i < buttons.length; ++i) {
        if (buttons[i].innerHTML.includes(translate('ru', 'Origin'))) {
            buttons[i].click();
            return;
        }
    }
}

function clickMoreInfo(node) {
    var buttons = node.getElementsByTagName('button');
    for (var i = 0; i < buttons.length; ++i) {
        if (buttons[i].innerHTML.includes(translate('ru', 'More'))) {
            buttons[i].click();
            return;
        }
    }
}

function clickStrength(node) {
    var buttons = node.getElementsByTagName('button');
    for (var i = 0; i < buttons.length; ++i) {
        if (buttons[i].innerHTML == translate('ru', 'Strength')) {
            buttons[i].click();
            return;
        }
    }
}

function clickStretch(node) {
    var buttons = node.getElementsByTagName('button');
    for (var i = 0; i < buttons.length; ++i) {
        if (buttons[i].innerHTML == translate('ru', 'Stretch')) {
            buttons[i].click();
            return;
        }
    }
}

async function clickInputs(node, fileIdx) {
    var muiFormControls = getMuiFormControls(node)[0];
    var mainInputs = getInputs(muiFormControls);
    
    var firstLayer = [];
    for (var i = 0; i < mainInputs.length; ++i) {
        var mainInputDescription = mainInputs[i].parentElement.parentElement.parentElement.children[1].innerHTML;
        mainInputs[i].click();
        var secondLayer = [];
        if (getMuiFormControls(node).length == 1) {
            imageUrls = [];
            try {
                var images = node.getElementsByTagName('img');
                for (var w = 0; w < images.length; ++w) {
                    var imageSrc = images[w].src;
                    console.log(imageSrc);
                    imageUrls.push(imageSrc);
                }
            }
            catch(e) {

            }
            var imageDescription = '';
            try {
                imageDescription = getImageDescription(images[0].parentElement.parentElement.children[1]);
                console.log(imageDescription);
            }
            catch(e) {

            }
            secondLayer.push({
                "movementName": subMainInputsDescription,
                "videoUrls" : [],
                "imageUrls" : imageUrls,
                "imageDescription" : imageDescription
            });
            firstLayer.push({
                "muscle": mainInputDescription,
                "movements": secondLayer
            });
            continue;
        }
        await delay(200);
        var subMuiFormControls = getMuiFormControls(node)[1];
        var subMainInputs = getInputs(subMuiFormControls);
        
        for (var j = 0; j < subMainInputs.length; ++j) {
            subMainInputs[j].click();
            await delay(200);
            var subMainInputsDescription = subMainInputs[j].parentElement.parentElement.parentElement.children[1].innerHTML;

            var muiPaperRootWithVideos = getMuiPaperRootWithVideos(node);
            var divWithVideoButtons = getDivWithVideoButtons(muiPaperRootWithVideos);
            var videoButtons = getButtonsFromDiv(divWithVideoButtons);

            imageUrls = [];
            try {
                var images = node.getElementsByTagName('img');
                for (var w = 0; w < images.length; ++w) {
                    var imageSrc = images[w].src;
                    console.log(imageSrc);
                    imageUrls.push(imageSrc);
                }
            }
            catch(e) {

            }

            var imageDescription = '';
            try {
                imageDescription = getImageDescription(images[0].parentElement.parentElement.children[1]);
                console.log(imageDescription);
            }
            catch(e) {

            }


            var videoUrls = [];
            for (var k = 0; k < videoButtons.length; ++k) {
                videoButtons[k].click();
                await delay(200);
                try {
                    var videos = muiPaperRootWithVideos.getElementsByTagName('video');
                    for (var w = 0; w < videos.length; ++w) {
                        var videoSrc = videos[w].src;
                        console.log(videoSrc);
                        videoUrls.push(videoSrc);
                    }
                }
                catch(e) {

                }
            }

            secondLayer.push({
                "movementName": subMainInputsDescription,
                "videoUrls" : videoUrls,
                "imageUrls" : imageUrls,
                "imageDescription" : imageDescription
            });
        }
        firstLayer.push({
            "muscle": mainInputDescription,
            "movements": secondLayer
        });
    }

    clickMoreInfo(document);
    await delay(900);

    var moreInfoText = '';
    try {
        moreInfoText = getImageDescription(document.getElementsByClassName('MuiDialog-container MuiDialog-scrollPaper')[0]);
        console.log(moreInfoText);
    }
    catch(e) {

    }

    clickStrength(document);
    await delay(900);

    var strengthVideoUrl = '';
    try {
        var videos = document.getElementsByTagName('video');
        if (videos.length > 0) {
            console.log(videos[1].src);
            strengthVideoUrl = videos[1].src;
        }
    }
    catch(e) {

    }

    clickStretch(document);
    await delay(900);

    var stretchVideoUrl = '';
    try {
        var videos = document.getElementsByTagName('video');
        if (videos.length > 0) {
            console.log(videos[1].src);
            stretchVideoUrl = videos[1].src;
        }
    }
    catch(e) {

    }

    clickOriginInsertion(document);
    await delay(900);

    var originAndInsertionVideoUrl = '';
    try {
        var videos = document.getElementsByTagName('video');
        if (videos.length > 0) {
            console.log(videos[1].src);
            originAndInsertionVideoUrl = videos[1].src;
        }
    }
    catch(e) {

    }

    var pageInfo = {
        "mainVideos": firstLayer,
        "moreInfoText": moreInfoText,
        "strengthVideoUrl": strengthVideoUrl,
        "stretchVideoUrl": stretchVideoUrl,
        "originAndInsertionVideoUrl": originAndInsertionVideoUrl
    }

    downloadText(`${fileIdx}.json`, JSON.stringify(pageInfo, null, 2));

    console.log(pageInfo);
}

function getMainApp(node) {
    var wrappers = node.getElementsByClassName('simplebar-wrapper');
    for (var i = 0; i < wrappers.length; ++i) {
        if (wrappers[i].innerHTML.includes(translate('ru', 'Videos:')) || wrappers[i].innerHTML.includes('Выберите мышцу:')) {
            return wrappers[i];
        }
    }
    return null;
}

function launchUrls() {
    for (var i = 96; i < 120; ++i) {
        // https://app.strength.muscleandmotion.com/submuscle/1
        window.open(`https://app.strength.muscleandmotion.com/submuscle/${i}`, '_blank');
    }
}

//var mainApp = getMainApp(document.body);
//clickInputs(mainApp, window.location.href.replace('https://app.strength.muscleandmotion.com/submuscle/', ''))

function getVideosContainer(node, name) {
    try {
        if (node.innerHTML.includes(name)) {
            if (node.getElementsByTagName('button').length > 0) {
                for (var i = 0; i < node.children.length; ++i) {
                    var result = getVideosContainer(node.children[i]);
                    if (result != null) {
                        return result;
                    }
                }
            }
            return node;
        }
        return null;
    }
    catch (e) {

    }
    return null;
}

async function clickInputs2(node, fileIdx) {
    var rightVideoContainer = getVideosContainer(node, 'Правильные видео:');
    var commonProblemsVideoContainer = getVideosContainer(node, 'Распространенные проблемы:');

    var containers = [rightVideoContainer, commonProblemsVideoContainer];

    var containersParsed = [];
    for (var i = 0; i < containers.length; ++i) {
        if (containers[i] != null) {
            var description = containers[i].children[0].innerHTML;
            var buttons = getButtonsFromDiv(containers[i]);
            var videosParsed = [];
            for (var j = 0; j < buttons.length; ++j) {
                buttons[j].click();
                await delay(200);
                var videoUrl = '';
                try {
                    videoUrl = node.getElementsByTagName('video')[0].src;
                }
                catch(e) {
                    
                }
                var videoDescription = '';
                try {
                    videoDescription = getImageDescription(node.getElementsByTagName('video')[0].parentElement.parentElement.parentElement.parentElement.children[1]);
                }
                catch(e) {
                    
                }
                var imageUrl = '';
                try {
                    imageUrl = node.getElementsByTagName('img')[0].src;
                }
                catch(e) {
                    
                }
                videosParsed.push({
                    "url": videoUrl,
                    "imageUrl": imageUrl,
                    "description": videoDescription
                })
            }
            containersParsed.push({
                "description": description,
                "videos": videosParsed
            });
        }
    }

    var muscleDescriptionsParsed = [];
    var buttons = getButtonsFromDiv(node);
    for (var i = 0; i < buttons.length; ++i) {
        if (buttons[i].innerHTML.includes('Мышцы')) {
            buttons[i].click();
            await delay(200);

            var buttonParent = buttons[i].parentElement;
            var muscleDescription = buttonParent.children[buttonParent.children.length - 1];

            for (var j = 0; j < muscleDescription.children.length; ++j) {
                var name = muscleDescription.children[j].children[0].textContent;
                var muscles = [];
                for (var k = 1; k < muscleDescription.children[j].children.length; ++k) {
                    muscles.push(getImageDescription(muscleDescription.children[j].children[k]));
                }
                muscleDescriptionsParsed.push({
                    "name": name,
                    "muscles": muscles
                })
            }

        }
    }

    

    var pageInfo = {
        "containers": containersParsed,
        "muscleDescriptions": muscleDescriptionsParsed
    }

    downloadText(`exercise_${fileIdx}.json`, JSON.stringify(pageInfo, null, 2));
}

var mainApp = document.getElementsByTagName('main')[0];
clickInputs2(mainApp, window.location.href.replace('https://app.strength.muscleandmotion.com/exercise/', ''))