function setupCanvas (display) {
    drawCanvas(display);
}
function updateDisplay (display, pc, i, dt, st, registers) {
    drawCanvas(display);
    updateData(pc, i, dt, st, registers);
}

function drawCanvas(display) {
    const canvas = document.getElementById('displayCanvas');
    const ctx = canvas.getContext('2d');
    const scale = 10;

    for (let y = 0; y < 32; y++) {
        for (let x = 0; x < 64; x++) {
            if (display[y * 64 + x] == 1) {
                ctx.fillStyle = "#616161";
            } else {
                ctx.fillStyle = "#EEEEEE";
            }
            ctx.fillRect(x * scale, y * scale, scale, scale);
        }
    }
}

function updateData(pc, i, dt, st, registers) {
    const pcl = document.getElementById('pc_label');
    pcl.innerHTML = "0x" + pc.toString(16).padStart(4, 0);

    const il = document.getElementById('i_label');
    il.innerHTML = "0x" + i.toString(16).padStart(4, 0);

    const dtl = document.getElementById('dt_label');
    dtl.innerHTML = "0x" + dt.toString(16).padStart(4, 0);

    const stl = document.getElementById('st_label');
    stl.innerHTML = "0x" + st.toString(16).padStart(4, 0);

    for (let i = 0; i < 16; i++) {
        let r = document.getElementById('v' + i);
        r.innerHTML = "0x" + registers[i].toString(16).padStart(2, 0);
    }
}

function addKeyListener(dotnetObj) {
    // Handle Keydown event
    document.addEventListener('keydown', (event) => {
        handleKeypressStatus(event.key, "down");
    });
    // Handle Keyup event
    document.addEventListener('keyup', (event) => {
        handleKeypressStatus(event.key, "up");
    });
    function handleKeypressStatus(key, state) {
        if (key == '1' || key == '2' || key == '3' || key == '4' ||
            key == 'q' || key == 'w' || key == 'e' || key == 'r' ||
            key == 'a' || key == 's' || key == 'd' || key == 'f' ||
            key == 'z' || key == 'x' || key == 'c' || key == 'v') {
            if (state == "down") {
                let keypad = document.getElementById('key' + key);
                keypad.setAttribute('style', 'background-color: #616161; color: #FFFFFF;');
                dotnetObj.invokeMethodAsync('HandleKeydown', key);
            }
            else {
                let keypad = document.getElementById('key' + key);
                keypad.removeAttribute('style');
                dotnetObj.invokeMethodAsync('HandleKeyup', key);
            }
        }
    }
    //DotNet.invokeMethodAsync('chip8wasm', 'HandleKeyup', event.key);
}
