
importClass(java.io.OutputStream);
importClass(java.net.Socket);
importClass(java.net.ServerSocket);

let engine = null;
let _engines = engines.all();

for (let i = 0; i < _engines.length; i++) {
    if (_engines[i].getSource().toString().indexOf("cap_script") != -1) {
        engine = _engines[i];
    }
}

if (engine == null) {
    alert("获取常驻脚本对象失败, 请在图色助手重新连接aj!");
    exit();
}

let img = engine.getRuntime().images.captureScreen();


function int2Bytes(num) {
    let bytes = java.lang.reflect.Array.newInstance(java.lang.Byte.TYPE, 4);
    let arr = new Array(); arr[0] = (num >> 24) & 0xff; arr[1] = (num >> 16) & 0xff; arr[2] = (num >> 8) & 0xff; arr[3] = num & 0xff;
    bytes[0] = arr[0] < 127 ? arr[0] : arr[0] - 256; bytes[1] = arr[1] < 127 ? arr[1] : arr[1] - 256;
    bytes[2] = arr[2] < 127 ? arr[2] : arr[2] - 256; bytes[3] = arr[3] < 127 ? arr[3] : arr[3] - 256;
    return bytes;
}


let socket;
let stream;
let remoteIP;

try {
    socket = new Socket(remoteIP, 5678);
    stream = socket.getOutputStream();

    let img_data = images.toBytes(img);

    let img_len = int2Bytes(img_data.length);

    stream.write(img_len);
    stream.write(img_data);
}
catch (error) {
    print(error);
}

if (stream != null) {
    stream.close();
}

if (socket != null) {
    socket.close();
}

exit();