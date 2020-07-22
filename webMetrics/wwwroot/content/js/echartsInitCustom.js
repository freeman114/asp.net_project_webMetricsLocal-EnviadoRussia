/// docs --> https://echarts.apache.org/en/tutorial.html

var $nameLegend = ['Followers'];
var $nameyAxis = 'Followers';
var $dias = ['01/07', '02/07', '03/07', '04/07', '05/07', '06/07', '07/07', '08/07', '09/07', '10/07', '11/07', '12/07', '13/07', '14/07', '15/07', '16/07', '17/07', '18/07', '19/07', '20/07', '21/07', '22/07', '23/07', '24/07', '25/07', '26/07', '27/07', '28/07', '29/07', '30/07',];
var $valores = [0, 11, 12, 12, 7, 10, 41, 23, 33, 11, 22, 12, 13, 9, 12, 12, 6, 7, 12, 15];

/* essas opções são bem especificas apra esse grafico, mas ainda tem opções globais... 
não sei como daria para padronizar... pois no fim ainda precisa re-iniciar o echart */
var chartOption = {
    baseOption: {
        ///color: $colors,
        color: $colorsTheme,
        grid: {
            show: false,
            ///left: '8%',
            right: '4%',
            bottom: '8%',
            containLabel: true
        },
        textStyle: {
            fontFamily: 'Montserrat',
            color: $colorsTheme[6],
        },
        tooltip: {
            trigger: 'axis',
            axisPointer: {
                type: 'line',
                lineStyle: {
                    type: 'dashed',
                },
            },
            ///enterable: true,
            textStyle: {
                color: $colorsTheme[6],
            },
        },
        legend: {
            data: $nameLegend,
        },

        xAxis: {
            type: 'category',
            scale: true,
            axisLine: {
                show: false,
                ///symbolSize: [20,20],
            },
            axisTick: {
                length: 20,
                ///interval: 4,
                lineStyle: {
                    color: 'rgba(255,255,255,.2)',
                    shadowOffsetY: 22
                }
            },
            splitLine: {
                show: false,
            },
            data: $dias,
        },
        yAxis: {
            type: 'value',
            name: $nameyAxis,
            nameTextStyle: {
                color: 'rgba(0,0,0,.45)',
                fontSize: 20,
            },
            axisLine: {
                show: false,
            },
            axisTick: {
                show: false,
            },
            splitLine: {
                lineStyle: {
                    color: $colorsTheme[8],
                    width: 2,
                },
            },
        },
        series: [{
            data: $valores,
            type: 'line',
            symbolSize: 10,
            label: {
                show: true,
                color: $colorsTheme[7],
                fontWeight: '400',
                fontSize: 14,
                ///distance: 10,
                textBorderColor: '#fff',
                textBorderWidth: 4,
            },
            itemStyle: {
                borderWidth: 2,
            },
            lineStyle: {
                width: 4,
            },
        }]
    },
    media: [ // each rule of media query is defined here
        {
            option: {
                grid: {
                    left: '4%',
                },
                tooltip: {
                    position: ['42.5%', 'top'],
                    confine: true,
                    axisPointer: {
                        ///type: 'line',
                        lineStyle: {
                            color: $colorsTheme[8],
                            opacity: 0.54,
                        },
                    },
                    formatter: '{c0} new followers<br /><small>{b0}</small>',
                    extraCssText: "font-size: 16px; background-color: #fff; box-shadow: 0 3px 6px 0 rgba(0, 0, 0, 0.16); padding: 8px; font-weight: bolder; ",
                },
                xAxis: {
                    axisLabel: {
                        fontWeight: '400',
                        fontSize: 12,
                        interval: 'auto',
                    },
                    offset: 12,
                    axisTick: {
                        show: false,
                    }
                },
                yAxis: {
                    nameGap: 24,
                    nameLocation: 'end',
                    nameRotate: 0,
                    nameTextStyle: {
                        fontSize: 14,
                    },
                    axisLabel: {
                        fontSize: 12,
                    },
                    offset: 4,
                },
            }
        },
        { //// media 1200
            query: { minWidth: 980 },   // write rule here
            option: {       // write options accordingly
                grid: {
                    left: '6%',
                },
                tooltip: {
                    position: 'top',
                    confine: false,
                    axisPointer: {
                        lineStyle: {
                            color: $colorsTheme[0],
                            ///opacity: 0.5,
                        },
                    },
                    formatter: '{c0} new followers<br /><small>{b0}</small>',
                    ///formatter: '{c0} new followers', /// nao sei se da pra colocar o texto para o top followers
                    extraCssText: "font-size: 18px; background-color: #fff; box-shadow: 0 3px 6px 0 rgba(0, 0, 0, 0.16); padding: 8px; font-weight: bolder; margin: -5.5% 0 0 -7.5%",
                },
                xAxis: {
                    axisLabel: {
                        fontWeight: '500',
                        fontSize: 12,
                        interval: 1,
                    },
                    offset: 16,
                    axisTick: {
                        show: true,
                    }
                },
                yAxis: {
                    nameLocation: 'center',
                    nameRotate: 90,
                    nameGap: 48,
                    nameTextStyle: {
                        color: 'rgba(0,0,0,.74)',
                        fontSize: 18,
                    },
                    axisLabel: {
                        fontSize: 14,
                    },
                    offset: 6,
                },
            }
        },
        { //// media 1200 - o ajuste eh pelo container e não pela resolução
            query: { minWidth: 1324 },   // write rule here
            option: {       // write options accordingly
                grid: {
                    left: '8%',
                },
                xAxis: {
                    axisLabel: {
                        fontSize: 16,
                    },
                },
                yAxis: {
                    nameGap: 64,
                    nameTextStyle: {
                        fontSize: 20,
                    },
                    axisLabel: {
                        fontSize: 16,
                    },
                    offset: 8,
                },
            }
        }
    ]
};


var growFollows = echarts.init(document.getElementById('containerGrowth'));

/* aqui inicia o grafico de verdade*/
growFollows.setOption(chartOption);


/* e aqui que garante que o grafico sempre seja responsivo */
$(function () {
    // Resize chart on menu width change and window resize
    $(window).on('resize', resize);
    ///$(".menu-toggle").on('click', resize);

    // Resize function
    function resize() {
        setTimeout(function () {

            // Resize chart
            /*precisava achar um jeito de isso ser generico para todos echarts que aparecerem na tela, o comum entre eles sempre sera a classe [echarts]*/
            growFollows.resize();
        }, 200);
        /* !!!!!!!!!!! não tirar do setTimeout - isso garante que o browser não seja sobrecarregado e comece a gerar erros*/
    }
});