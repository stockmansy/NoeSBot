var path = require("path");
module.exports = {
    entry: [
        "babel-polyfill",
        "./Scripts/app/component/featureComponent"
    ],
    output: {
        publicPath: "/js/",
        path: path.join(__dirname, "/wwwroot/js/"),
        filename: "app.build.js"
    },
    module: {
        loaders: [{
            exclude: /node_modules/,
            loader: "babel-loader"
        }]
    },
    resolve: {
        alias: {
            vue: 'vue/dist/vue.js'
        }
    }
};